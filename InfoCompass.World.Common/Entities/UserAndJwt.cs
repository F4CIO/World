namespace InfoCompass.World.Common.Entities;

[Serializable]
public class UserAndJwt:User
{
	public User User { get; set; }
	public string Jwt { get; set; }
	public DateTime JwtExpirationMomentAsUtc { get; set; }

	public UserAndJwt(User user, string jwt, DateTime jwtExpirationMoment)
	{
		this.User = user;
		this.Jwt = jwt;
		this.JwtExpirationMomentAsUtc = jwtExpirationMoment;
	}
}
