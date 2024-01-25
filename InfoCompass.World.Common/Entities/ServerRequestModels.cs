namespace InfoCompass.World.Common.Entities;

public class GetLoggedInUserOrNullRequest
{

}

[Serializable]
public class LoginRequest
{
	public string Username { get; set; }
	public string Password { get; set; }
	//public string CookieOrJwt { get; set; }

	public void Trim()
	{
		this.Username = this.Username?.Trim();
		this.Password = this.Password?.Trim();
	}

	public Errors Validate(ServiceForCOE c, Errors errors = null)
	{
		errors = errors ?? new Errors();

		FieldValidator.ValidateField(this.Username, "Username", "Email", FieldType.EMail, true, false, 3, 255, errors);
		FieldValidator.ValidateField(this.Password, "Password", "Password", FieldType.Other, true, false, 5, 255, errors);
		//if(this.CookieOrJwt.ToLower() != "cookie" && this.CookieOrJwt.ToLower() != "jwt")
		//{
		//	errors.Add("CookieOrJwt", "CookieOrJwt must be either Cookie or Jwt");
		//}

		return errors;
	}
}

[Serializable]
public class ChangePasswordRequest
{
	public long UserId { get; set; }
	public string OldPassword { get; set; }
	public string NewPassword { get; set; }
	public string NewPasswordRepeated { get; set; }

	public void Trim()
	{
		this.OldPassword = this.OldPassword?.Trim();
		this.NewPassword = this.NewPassword?.Trim();
		this.NewPasswordRepeated = this.NewPasswordRepeated?.Trim();
	}

	public Errors Validate(ServiceForCOE c, Errors errors = null)
	{
		errors = errors ?? new Errors();

		FieldValidator.ValidateField(this.OldPassword, "OldPassword", "Old Password", FieldType.Other, true, false, 5, 255, errors);
		FieldValidator.ValidateField(this.OldPassword, "NewPassword", "New Password", FieldType.Other, true, false, 5, 255, errors);
		FieldValidator.ValidateField(this.OldPassword, "NewPasswordRepeated", "New Password Confirmation", FieldType.Other, true, false, 5, 255, errors);
		if(this.NewPassword != this.NewPasswordRepeated)
		{
			errors.Add("NewPasswordRepeated", "New Password and New Password Confirmation do not match");
		}

		return errors;
	}
}


[Serializable]
public class ChangePasswordUsingTokenRequest
{
	public string NewPassword { get; set; }
	public string NewPasswordRepeated { get; set; }
	public string Token { get; set; }

	public void Trim()
	{
		this.NewPassword = this.NewPassword?.Trim();
		this.NewPasswordRepeated = this.NewPasswordRepeated?.Trim();
		this.Token = this.Token?.Trim();
	}

	public Errors Validate(ServiceForCOE c, Errors errors = null)
	{
		errors = errors ?? new Errors();

		FieldValidator.ValidateField(this.NewPassword, "NewPassword", "New Password", FieldType.Other, true, false, 5, 255, errors);
		FieldValidator.ValidateField(this.NewPasswordRepeated, "NewPasswordRepeated", "Repeated New Password", FieldType.Other, true, false, 5, 255, errors);
		if(this.NewPassword != this.NewPasswordRepeated)
		{
			errors.Add("NewPasswordRepeated", "Typed two passwords do not match");
		}

		return errors;
	}
}

[Serializable]
public class RegisterRequest
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string EMail { get; set; }
	public bool? SubscribeMe { get; set; }
	public string NewPassword { get; set; }
	public string NewPasswordRepeated { get; set; }
	public bool? IAgreeToTerms { get; set; }

	public void Trim()
	{
		FirstName = FirstName?.Trim();
		LastName = LastName?.Trim();
		EMail = EMail?.Trim();
		NewPassword = NewPassword?.Trim();
		NewPasswordRepeated = NewPasswordRepeated?.Trim();
	}

	public Errors Validate(ServiceForCOE c, Errors errors = null)
	{
		errors = errors ?? new Errors();

		FieldValidator.ValidateField(FirstName, "FirstName", "First Name", FieldType.AlphaAnyLanguage, false, false, 1, 100, errors);
		FieldValidator.ValidateField(LastName, "LastName", "Last Name", FieldType.AlphaAnyLanguage, false, false, 1, 100, errors);
		FieldValidator.ValidateField(EMail, "EMail", "Email", FieldType.EMail, true, false, 1, 255, errors);
		FieldValidator.ValidateField(NewPassword, "NewPassword", "New Password", FieldType.Other, true, false, 5, 255, errors);
		FieldValidator.ValidateField(NewPasswordRepeated, "NewPasswordRepeated", "Repeated New Password", FieldType.Other, true, false, 5, 255, errors);

		if(NewPassword != NewPasswordRepeated)
		{
			errors.Add("NewPasswordRepeated", "Typed two passwords do not match");
		}

		if(IAgreeToTerms != true)
		{
			errors.Add("IAgreeToTerms", "You must agree to Terms if you want to proceed");
		}

		return errors;
	}
}