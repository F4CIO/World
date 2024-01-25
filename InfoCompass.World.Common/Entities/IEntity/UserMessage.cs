namespace InfoCompass.World.Common.Entities;
public class UserMessage:IEntity
{
	public long Id { get; set; }
	public long UserId { get; set; }
	public string UserEMail { get; set; }
	public DateTime MomentOfCreation { get; set; }
	public DateTime MomentOfLastUpdate { get; set; }
	public string Message { get; set; }
	public bool IsRead { get; set; }
	public bool IsVisible { get; set; }
	public string Note { get; set; }

	public long ThreadId { get; set; }
	public long PreviousMessageId { get; set; }
	public long NextMessageId { get; set; }

	public void Trim()
	{
		this.UserEMail = this.UserEMail?.Trim();
		this.Message = this.Message?.Trim();
		this.Note = this.Note?.Trim();
		//other fields here once admin can edit users
	}

	public Errors Validate(ServiceForCOE c, Errors errors = null)
	{
		errors = errors ?? new Errors();

		FieldValidator.ValidateField(this.UserEMail, "UserEMail", "User EMail", FieldType.EMail, false, false, 0, 100, errors);
		FieldValidator.ValidateField(this.Message, "Message", "Message", FieldType.Other, true, true, 20, 1000, errors);
		FieldValidator.ValidateField(this.Note, "Note", "Note", FieldType.Other, false, true, 0, 1000, errors);
		//other fields here once admin can edit users

		return errors;
	}

	public string ToString(string separator = ",")
	{
		var sb = new StringBuilder();

		sb.Append($"Id: {this.Id}");
		sb.Append($"{separator}UserId: {this.UserId.ToString()?.ToNonNullString("null")}");
		sb.Append($"{separator}UserEMail: {this.UserEMail.ToString()?.ToNonNullString("null")}");
		sb.Append($"{separator}MomentOfCreation: {this.MomentOfCreation.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}");
		sb.Append($"{separator}MomentOfLastUpdate: {this.MomentOfLastUpdate.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}");
		sb.Append($"{separator}Message: {this.Message.ToNonNullString("null")}");
		sb.Append($"{separator}IsRead: {this.IsRead.ToYesOrNoString()}");
		sb.Append($"{separator}IsVisible: {this.IsVisible.ToYesOrNoString()}");
		sb.Append($"{separator}Note: {this.Note.ToNonNullString("null")}");

		sb.Append($"{separator}ThreadId: {this.ThreadId}");
		sb.Append($"{separator}PreviousMessageId: {this.PreviousMessageId}");
		sb.Append($"{separator}NextMessageId: {this.NextMessageId}");

		return sb.ToString();
	}
}
