using System;

[assembly: CLSCompliant(false)]

namespace GtMotive.Estimate.Microservice.Host.Configuration
{
    public sealed class AppSettings
    {
        public string JwtAuthority { get; set; }
    }
}
