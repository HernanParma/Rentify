namespace Domain.Entities
{
    public static class CustomClaims
    {
        public const string CanEditOwnProfile = "CanEditOwnProfile";
        public const string CanViewBranchInfo = "CanViewBranchInfo";
        public const string CanManageReservations = "CanManageReservations";
        public const string CanViewOwnReservations = "CanViewOwnReservations";
        public const string CanManageFleet = "CanManageFleet";
        public const string CanManageBranches = "CanManageBranches";

        public const string UserId = "UserId";
        public const string UserEmail = "UserEmail";
        public const string UserRole = "UserRole";
        public const string IsEmailVerified = "IsEmailVerified";
        public const string AccountStatus = "AccountStatus";
        public const string CustomerId = "CustomerId";
        public const string DriverLicenseNumber = "DriverLicenseNumber";
    }
}
