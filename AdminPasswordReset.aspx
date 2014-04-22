<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Import Namespace="Sitecore" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sitecore Admin Password Reset</title>
</head>
<body>
    <%
        string resetPassword = String.Empty;
        string userName = string.Empty;
        string newPassword = "newPassword";
        bool status = false;
        bool isLocked = false;

        using (new Sitecore.SecurityModel.SecurityDisabler())
        {
            userName = @"sitecore\admin";
            MembershipUser user = Membership.GetUser(userName, true);
            if (user != null)
            {
                resetPassword = user.ResetPassword();
                status = user.ChangePassword(resetPassword, newPassword);
                isLocked = user.IsLockedOut;
                if (isLocked)
                {
                    user.UnlockUser();
                }
            }
        }

        Response.Write(String.Format("Password updated <b>{0}</b> for user - {1}", (status ? "successfully" : "failed"), userName));
    %>
</body>
</html>
