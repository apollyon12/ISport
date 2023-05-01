using System.Collections;
using System.Globalization;

using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;

using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using iSportsRecruiting.Shared.Services;

using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Dapper;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class CreditCardController : BaseApiController<CreditCardController>
    {
        // This needs to go to the DB and be more complete. Control all ways of payment from values of each plan
        private static readonly IReadOnlyDictionary<string, lineItemType> Plans = new Dictionary<string, lineItemType>
        {
            ["WS"] = new() {itemId = "WS", name = "Weekly Subs", quantity = 1, unitPrice = new decimal(7.99)},
            ["MS"] = new() {itemId = "MS", name = "Monthly Subs", quantity = 1, unitPrice = new decimal(24.99)},
            ["BE"] = new() { itemId = "BE", name = "Blast Email", quantity = 1, unitPrice = new decimal(99) },
            ["PC"] = new() { itemId = "PC", name = "Advisor", quantity = 1, unitPrice = new decimal(199) },
            ["VE"] = new() { itemId = "VE", name = "Video Editing", quantity = 1, unitPrice = new decimal(99) },
            ["30DF"] = new() { itemId = "30DF", name = "30 days free, then $24.99/month.", quantity = 1, unitPrice = new decimal(24.99) }
        };

        public static async Task<messagesType?> PayAsync(IDatabaseContext context,
            CreditCardTransactionDTO creditCardTransaction, string plan, string? promoCode = null,
            bool firstPayment = true, bool isTempOffer = false)
        {
            if (!Plans.ContainsKey(plan))
                return null;

            lineItemType item = Plans[plan];

            if(isTempOffer)
            {
                item.unitPrice = new decimal(1);
            }

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
            
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = "9PLyn36VV",
                ItemElementName = ItemChoiceType.transactionKey,
                Item = "5F93sG4pN6GD3q69"
            };
            
            item.description += $"{item.name} - {creditCardTransaction.FirstName} {creditCardTransaction.LastName}";
            
            var transactionRequest = new transactionRequestType
            {
                amount = item.unitPrice,
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                billTo = new customerAddressType
                {
                    firstName = creditCardTransaction.FirstName,
                    lastName = creditCardTransaction.LastName,
                    city = creditCardTransaction.City,
                    address = creditCardTransaction.Address,
                    zip = creditCardTransaction.Zip
                },
                payment =  new paymentType
                {
                    Item = new creditCardType
                    {
                        cardNumber = creditCardTransaction.CardNumber?.Replace(" ", string.Empty),
                        expirationDate = creditCardTransaction.ExpirationDate,
                        cardCode = creditCardTransaction.CardCode,
                    }
                },
                lineItems = new[] {item},
            };


            // This needs to be changed to be more flexible and assign unit price based on PromotionCode model.
            if (!isTempOffer && promoCode is not null)
            {
                var promotion = await context.GetPromotionByCodeAsync(promoCode);

                // hack.. should go by type (needs to be added) - Promo Code should have TYPES like DISCOUNT TYPE, FREE FOR X DAYS TYPE, REGULAR TYPE, etc.
                if (promotion is not null && promotion.Code != "30DAYSFREE")
                {
                    item.unitPrice -= (item.unitPrice * promotion.Discount) / 100;
                }
                else if (promotion != null && promotion.Code == "30DAYSFREE")
                {
                    // no assignment needed, already at unit price without discounts.
                }
            }


            // This needs to be controlled by (PLAN) TYPE not individual PROMOTIONAL CODES
            if (promoCode != null && promoCode.Trim().ToUpper() == "30DAYSFREE")
            {
                // values (should be set in PLAN)
                short intervalLength = 1; // interval payment = once a month
                //short freeDays = 30;      // after 30 days start payment 

                // SET INTERVALS for SUBSCRIPTION
                paymentScheduleTypeInterval interval = new paymentScheduleTypeInterval();

                interval.length = intervalLength;                        // months can be indicated between 1 and 12
                interval.unit = ARBSubscriptionUnitEnum.months;

                paymentScheduleType schedule = new paymentScheduleType
                {
                    interval = interval,
                    startDate = DateTime.Now,      // start date should be tomorrow
                    totalOccurrences = 9999,                          // 999 indicates no end date
                    trialOccurrences = 1
                };

                nameAndAddressType addressInfo = new nameAndAddressType()
                {
                    firstName = creditCardTransaction.FirstName,
                    lastName = creditCardTransaction.LastName
                };

                var cc = new paymentType
                {
                    Item = new creditCardType
                    {
                        cardNumber = creditCardTransaction.CardNumber?.Replace(" ", string.Empty),
                        expirationDate = creditCardTransaction.ExpirationDate?.Replace("/", ""),
                        cardCode = creditCardTransaction.CardCode,
                    }
                };

                ARBSubscriptionType subscriptionType = new ARBSubscriptionType()
                {
                    amount = item.unitPrice,
                    trialAmount = 0.00m,
                    paymentSchedule = schedule,
                    billTo = addressInfo,
                    payment = cc
                };

                var request = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

                var controller = new ARBCreateSubscriptionController(request);
                controller.Execute();

                ARBCreateSubscriptionResponse response = controller.GetApiResponse();   // get the response from the service (errors contained if any)

                // validate response
                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    response.messages.message = new[]
                        {
                        new messagesTypeMessage
                            {code = "UnitPrice", text = item.unitPrice.ToString(CultureInfo.InvariantCulture) }
                        };

                    return response.messages;
                }
                else if (response != null)
                {
                    throw new ApplicationException("There was an error while registering your account. Please contact our administrator.");
                }

            }
            else
            {
                var request = new createTransactionRequest { transactionRequest = transactionRequest };
                var controller = new createTransactionController(request);
                controller.Execute();

                var response = controller.GetApiResponse();

                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        var requestDetails = new getTransactionDetailsRequest { transId = response.transactionResponse.transId };
                        var controllerDetails = new getTransactionDetailsController(requestDetails);
                        controllerDetails.Execute();

                        var responseDetails = controllerDetails.GetApiResponse();

                        if (responseDetails.transaction.responseReasonCode != 1)
                            throw new Exception(responseDetails.transaction.responseReasonDescription);

                        response.messages.message = new[]
                        {
                        new messagesTypeMessage
                            {code = "UnitPrice", text = item.unitPrice.ToString(CultureInfo.InvariantCulture)}
                    };

                        return response.messages;
                    }
                }
            }

            return null;
        }

        [HttpPost("{entityId:int}/{plan}")]
        public async Task<ActionResult> Post(CreditCardTransactionDTO creditCardTransaction, int entityId, string plan,
            [FromQuery] string promoCode = null, [FromQuery] bool firstPayment = true)
        {
            var payment = await PayAsync(_context, creditCardTransaction, plan, promoCode: promoCode,
                firstPayment: firstPayment);

            if (payment is null)
                return Problem("This plan doesn't exist");

            if (payment.resultCode == messageTypeEnum.Ok)
            {
                var unitPrice = payment.message.First(m => m.code == "UnitPrice").text;

                await _context.UpdateBillingPlanAsync(entityId, plan);
                await _context.PostPaymentByEntityId(new EntityPayments
                {
                    Entity_Id = entityId, Plan = plan, Amount = unitPrice, Date = DateTime.Now,
                    Promotion_Code = promoCode
                });

                return Ok();
            }

            return Problem(string.Join("\n", payment.message.Select(m => m.text)));
        }


        [HttpGet("payments/{entityId:int}")]
        public async Task<ActionResult> GetPayments(int entityId)
        {
            try
            {
                var result = await _context.GetPaymentsByEntityIdAsync(entityId);

                return Ok(new Response<IEnumerable<EntityPayments>>
                {
                    Payload = result
                });
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        // [HttpPost("payments/{entityId:int}/{plan}")]
        // public async Task<ActionResult> PostPaymentInfo(int entityId, string plan, EntityPayments billing)
        // {
        //     try
        //     {
        //         await _context.UpdateEntityBillingAsync(entityId, plan, billing);
        //
        //         return Ok();
        //     }
        //     catch (Exception e)
        //     {
        //         return Problem(e.Message);
        //     }
        // }

        [HttpGet("billing/{entityId:int}")]
        public async Task<ActionResult> GetBillingInfo(int entityId)
        {
            try
            {
                var result = await _context.GetBillingByEntityIdAsync(entityId);

                return Ok(new Response<EntityBilling>
                {
                    Payload = result
                });
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("billing/{entityId:int}/{plan}")]
        public async Task<ActionResult> PostBillingInfo(EntityBilling billing, string plan, int entityId)
        {
            try
            {
                await _context.UpdateEntityBillingAsync(entityId, plan, billing);

                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}