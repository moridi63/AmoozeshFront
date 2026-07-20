// Core/Extensions/CertificateStatusExtensions.cs
using AmoozeshFront.Core.Enums;

namespace AmoozeshFront.Core.Extensions
{
    public static class CertificateStatusExtensions
    {
        public static string ToDisplayName(this CertificateStatus status) => status switch
        {
            CertificateStatus.Pending => "در انتظار",
            CertificateStatus.Approved => "تایید شده",
            CertificateStatus.Rejected => "رد شده",
            CertificateStatus.UnderReview => "در حال بررسی",
            CertificateStatus.Printed => "چاپ شده",
            _ => "نامشخص" 
        };

        public static string ToBadgeClass(this CertificateStatus status) => status switch
        {
            CertificateStatus.Pending => "bg-secondary-subtle text-secondaryborder border border-secondary-subtle",
            CertificateStatus.Approved => "bg-success-subtle text-success border border-success-subtle",
            CertificateStatus.Rejected => "bg-danger-subtle text-danger border border-danger-subtle",
            CertificateStatus.UnderReview => "bg-warning-subtle text-warning-emphasis border border-warning-subtle",
            CertificateStatus.Printed => "bg-primary-subtle text-primary border border-primary-subtle",
            _ => "bg-body-secondary text-body-secondary"
        };
    }
}