namespace InfoCompass.World.Common.Entities;

[Serializable]
public class User:IEntity
{
	public long Id { get; set; }
	public string EMail { get; set; }
	public string? PasswordHash { get; set; }
	public DateTime MomentOfCreation { get; set; }
	public DateTime MomentOfLastUpdate { get; set; }
	public DateTime? MomentOfLastLogin { get; set; }
	public bool IsRegistered { get; set; }
	public bool IsAdministrator { get; set; }
	public bool IsActive { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }

	public string FirstNameAndLastName
	{
		get
		{
			string r = (((this?.FirstName) ?? "") + " " + ((this?.LastName) ?? "")).Trim();
			return r;
		}
	}

	public DateTime? MomentOfLastVisit { get; set; }
	public bool Subscribed { get; set; }

	public long? PaidPlanId { get; set; }
	public string Note { get; set; }

	public void Trim()
	{
		this.FirstName = this.FirstName?.Trim();
		this.LastName = this.LastName?.Trim();
		//other fields here once admin can edit users
	}

	public Errors Validate(ServiceForCOE c, Errors errors = null)
	{
		errors = errors ?? new Errors();

		FieldValidator.ValidateField(this.FirstName, "FirstName", "First Name", FieldType.AlphaAnyLanguage, false, true, 1, 100, errors);
		FieldValidator.ValidateField(this.LastName, "LastName", "Last Name", FieldType.AlphaAnyLanguage, false, true, 1, 100, errors);
		//other fields here once admin can edit users

		return errors;
	}
}

public class UserFilter:User
{
	public bool FilterBy_Id;
	public bool FilterBy_Email;
	public bool FilterBy_PasswordHash;
	public bool FilterBy_MomentOfCreation;
	public bool FilterBy_MomentOfLastUpdate;
	public bool FilterBy_MomentOfLastLogin;
	public bool FilterBy_IsRegistered;
	public bool FilterBy_IsAdministrator;
	public bool FilterBy_IsActive;
	public bool FilterBy_FirstName;
	public bool FilterBy_LastName;
	public bool FilterBy_MomentOfLastVisit;
	public bool FilterBy_Subscribed;
	public bool FilterBy_PaidPlan;
	public bool FilterBy_Note;
}


