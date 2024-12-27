namespace MyCompany.World.Common.Entities;
public class RegistrationConfirmation:IEntity
{
	public long Id { get; set; }
	public long UserId { get; set; }
	public DateTime MomentOfCreation { get; set; }
	public DateTime MomentOfLastUpdate { get; set; }
	public string RawData { get; set; }
	public DateTime MomentOfExpirationAsUtc { get; set; }
	public bool IsActive { get; set; }
	public string ReasonForDeactivation { get; set; }


	public string PendingFirstName { get; set; }
	public string PendingLastName { get; set; }
	public string PendingEMail { get; set; }
	public bool? PendingSubscribeMe { get; set; }
	public string PendingPasswordHash { get; set; }
	public bool? PendingIAgreeToTerms { get; set; }



	public override string ToString()
	{
		var sb = new StringBuilder();

		sb.Append($"Id: {this.Id}");
		sb.Append($",UserId: {this.UserId.ToString()?.ToNonNullString("null")}");
		sb.Append($",MomentOfCreation: {this.MomentOfCreation.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}");
		sb.Append($",MomentOfLastUpdate: {this.MomentOfLastUpdate.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}");
		sb.Append($",RawData: {this.RawData.ToNonNullString("null")}");
		sb.Append($",MomentOfExpirationAsUtc: {this.MomentOfExpirationAsUtc.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}");
		sb.Append($",IsActive: {this.IsActive.ToYesOrNoString()}");
		sb.Append($",ReasonForDeactivation: {this.ReasonForDeactivation.ToNonNullString("null")}");

		sb.Append($"PendingFirstName: {this.PendingFirstName}");
		sb.Append($"PendingLastName: {this.PendingLastName}");
		sb.Append($"PendingEMail: {this.PendingEMail}");
		sb.Append($"PendingSubscribeMe: {this.PendingSubscribeMe}");
		sb.Append($"PendingPasswordHash: {this.PendingPasswordHash}");
		sb.Append($"PendingIAgreeToTerms: {this.PendingIAgreeToTerms}");

		return sb.ToString();
	}
}

public enum RegistrationConfirmationReasonForDeactivation
{
	Expired,
	UserLogedOutBeforeExpiration,
	BadBehaviour,
	Other
}
