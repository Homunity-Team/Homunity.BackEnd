namespace Homunity_Web_Api.Authorization
{
    // أسماء الـPolicies الموحّدة المستخدمة في الـAuthorization المركزي (Sprint 2).
    public static class PolicyNames
    {
        // يُستخدم للتحقق من ملكية مصدر معيّن: المستخدم الحالي هو صاحب المصدر (أو أحد أصحابه)، أو Admin.
        public const string ResourceOwner = "ResourceOwner";

        // يُستخدم لتوحيد التحقق من صلاحيات الـAdmin بدلاً من تكرار [Authorize(Roles = "Admin")] كنص حرفي.
        public const string AdminOnly = "AdminOnly";
    }
}
