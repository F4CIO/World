namespace MyCompany.World.Common.Entities;
public class Jwt:IEntity
{
	public long Id { get; set; }
	public long UserId { get; set; }
	public DateTime MomentOfCreation { get; set; }
	public DateTime MomentOfLastUpdate { get; set; }
	public string RawData { get; set; }
	public DateTime MomentOfExpirationAsUtc { get; set; }
	public bool IsActive { get; set; }
	public string ReasonForDeactivation { get; set; }

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

		return sb.ToString();
	}
}

public enum JwtReasonForDeactivation
{
	Expired,
	UserLogedOutBeforeExpiration,
	BadBehaviour,
	Other
}
