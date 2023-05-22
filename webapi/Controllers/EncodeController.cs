using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AgustinJayConsolacion.Controllers
{
    [ApiController]
    [Route("api/encode")]
    public class EncodeController : ControllerBase
    {
        private CancellationTokenSource _cancellationTokenSource;
        private static readonly Dictionary<string, CancellationTokenSource> _requestCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        public class EncodeModel
        {
            public string EncodeText { get; set; }
        }

        private static string _reqID = string.Empty;

        [HttpPost]
        public async Task<IActionResult> Encode([FromBody] EncodeModel encodeModel)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                return BadRequest("Another encoding process is already in progress.");
            }
            _reqID = HttpContext.TraceIdentifier;
            _cancellationTokenSource = new CancellationTokenSource();

            var encodedText = Convert.ToBase64String(Encoding.UTF8.GetBytes(encodeModel.EncodeText));
            var returnStringBuilder = new StringBuilder();

            var cancellationToken = _cancellationTokenSource.Token;
            foreach (var character in encodedText)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Ok("Encoding process canceled.");
                }

                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 6)), cancellationToken);
                returnStringBuilder.Append(character);
            }

            return Ok(returnStringBuilder.ToString());
        }


        [HttpPost("cancel")]
        public IActionResult Cancel()
        {
            string a = _reqID;
            if (_requestCancellationTokenSources.TryGetValue(_reqID, out var cancellationTokenSource))
            {
                // Cancel the CancellationTokenSource
                cancellationTokenSource.Cancel();

                // Remove the CancellationTokenSource from the dictionary
                _requestCancellationTokenSources.Remove(_reqID);

                return Ok();
            }

            return Ok();
        }
    }

}
