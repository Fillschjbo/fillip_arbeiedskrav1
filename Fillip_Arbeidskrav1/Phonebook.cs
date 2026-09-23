namespace Fillip_Arbeidskrav1;

public class Phonebook
{
    public int Count => _contacts.Length;
    
    private readonly Contact[] _contacts;

    private Phonebook(Contact[] contacts)
    {
        _contacts = contacts;
    }

    public static Phonebook Load(string phonebook)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(phonebook);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException("Phonebook file not found", phonebook);
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException("Phonebook directory not found");
        }

        if (lines.Length < 2)
        {
            throw new FormatException("Phonebook file has no data rows");
        }

        var contacts = new Contact[lines.Length - 1];

        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(",");

            if (fields.Length != 6)
            {
                throw new FormatException($"Malformed row at line {i + 1}: expected 6 fields, got {fields.Length}");
            }

            DateTime birthday;
            try
            {
                birthday = DateTime.ParseExact(fields[3], "yyyy-MM-dd", null);
            }
            catch (FormatException)
            {
                throw new FormatException($"malformed birthday at line {i + 1}: {fields[3]}");
            }

            contacts[i - 1] = new Contact(
                firstName: fields[0],
                lastName: fields[1],
                mobileNumber: fields[2],
                birthday: birthday,
                street: fields[4],
                city: fields[5]);
        }

        return new Phonebook(contacts);
    }
}