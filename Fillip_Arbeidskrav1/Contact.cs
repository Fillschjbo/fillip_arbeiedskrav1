namespace Fillip_Arbeidskrav1;

public class Contact
{
    public string FirstName { get; }
    public string LastName { get;  }
    public string MobileNumber { get; }
    public DateTime Birthday { get; }
    public string Street { get; }
    public string City { get; }
    
    public Contact (string firstName, string lastName, string mobileNumber, DateTime birthday, string street, string city)
    {
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber;
        Birthday = birthday;
        Street = street;
        City = city;
    }
}