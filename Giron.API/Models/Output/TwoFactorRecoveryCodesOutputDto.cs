using System.Collections.Generic;

namespace Giron.API.Models.Output
{
    public class TwoFactorRecoveryCodesOutputDto
    {
        public IEnumerable<string> RecoveryCodes { get; set; }
    }
}