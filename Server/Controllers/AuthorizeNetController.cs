using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using iSportsRecruiting.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace iSportsRecruiting.Server.Controllers.v1
{
    public class AuthorizeNetController : BaseApiController<AuthorizeNetController>
    {
        [HttpPost]
        public ActionResult Post(CreditCardTransactionDTO creditCardTransaction)
        {
            try
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = "9PLyn36VV",
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = "5F93sG4pN6GD3q69",
                };

                var transactionRequest = new transactionRequestType
                {
                    amount = new decimal(0.01),
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                    billTo = new customerAddressType
                    {
                        firstName = creditCardTransaction.FirstName,
                        lastName = creditCardTransaction.LastName,
                        city = creditCardTransaction.City,
                        address = creditCardTransaction.Address,
                        zip = creditCardTransaction.Zip
                    },
                    payment = new paymentType
                    {
                        Item = new creditCardType
                        {
                            cardNumber = creditCardTransaction.CardNumber,
                            expirationDate = creditCardTransaction.ExpirationDate,
                            cardCode = creditCardTransaction.CardCode
                        }
                    },
                    lineItems = new[] { new lineItemType { itemId = "1", name = "ISR Plan", quantity = 1, unitPrice = new decimal(0.01) } },
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                var controller = new createTransactionController(request);
                controller.Execute();

                var response = controller.GetApiResponse();

                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        return Ok();
                    }
                }

                return Problem(string.Join(",", response.messages.message.Select(m => m.text)));
            }
            catch (Exception e)
            {

            }

            return Problem();
        }
    }
}
