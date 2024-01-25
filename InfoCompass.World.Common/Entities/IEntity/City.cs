namespace InfoCompass.World.Common.Entities;

[Serializable]
public class City:IEntity
{
	public long Id { get; set; }

	public DateTime MomentOfCreation { get; set; }
	public DateTime MomentOfLastUpdate { get; set; }
	public long UserId { get; set; }
	public User User { get; set; }
	public string UserEMailHint { get; set; }
	public string Name { get; set; }
}
