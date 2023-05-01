using System.Drawing;
using System.Drawing.Imaging;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Mvc;

namespace iSportsRecruiting.Server.Controllers
{
    public class FileController : BaseApiController<FileController>
    {
        [HttpGet("uploads/{height:int}/{width:int}/{description}")]
        public ActionResult ScaleFromUploads(int height, int width, string description)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //uploads or images
            var path = $"{baseDirectory}\\wwwroot\\uploads\\{description}";

            using (var img = Image.FromFile(path))
            {
                var stream = new MemoryStream();
                img.Scale(width, height).Save(stream, ImageFormat.Png);

                return File(stream.ToArray(), "image/png", description);
            }
        }

        [HttpGet("images/{height:int}/{width:int}/{description}")]
        public ActionResult ScaleFromImages(int height, int width, string description)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //uploads or images
            var path = $"{baseDirectory}\\wwwroot\\images\\{description}";

            using (var img = Image.FromFile(path))
            {
                var stream = new MemoryStream();
                img.Scale(width, height).Save(stream, ImageFormat.Png);

                return File(stream.ToArray(), "image/png", description);
            }
        }

        [HttpGet("logos/{height:int}/{width:int}/{description}")]
        public ActionResult ScaleFromLogos(int height, int width, string description)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //uploads or images
            var path = $"{baseDirectory}\\wwwroot\\images\\logos\\{description}";

            using (var img = Image.FromFile(path))
            {
                var stream = new MemoryStream();
                img.Scale(width, height).Save(stream, ImageFormat.Png);

                return File(stream.ToArray(), "image/png", description);
            }
        }
    }
}
