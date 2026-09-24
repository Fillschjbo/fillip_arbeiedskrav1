namespace Fillip_Arbeidskrav1;

public class Phonebook
{
    private int _comparisons;
    private int _sortComparisons;
    private int _sortSwaps;
    
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

    private static string Key(Contact contact, Field field)
    {
        return field switch
        {
            Field.Firstname => contact.FirstName,
            Field.Lastname => contact.LastName,
            Field.Mobile => contact.MobileNumber,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "unhandled field")
        };
    }

    private int CompareContacts(Contact a, Contact b, Field field, SortOrder order)
    {
        _sortComparisons++;
        int result = string.Compare(Key(a, field), Key(b, field), StringComparison.OrdinalIgnoreCase);
        return order == SortOrder.Ascending ? result : -result;
        
    }

    public Contact[] LinearSearch(Field field, string target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        _comparisons = 0;
        var matches = new List<Contact>();

        foreach (Contact contact in _contacts)
        {
            _comparisons++;
            if (string.Equals(Key(contact, field), target, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(contact);
            }
        }
        return matches.ToArray();
    }

    public Contact[] GetAll() => _contacts;

    public void BubbleSort(Field field, SortOrder order)
    {
        _sortComparisons = 0;
        _sortSwaps = 0;
        
        int n = _contacts.Length;
        for (int i = 0; i < n; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - 1; j++)
            {
                if (CompareContacts(_contacts[j], _contacts[j + 1], field, order) > 0)
                {
                    (_contacts[j], _contacts[j + 1]) = (_contacts[j + 1], _contacts[j]);
                    _sortSwaps++;
                    swapped = true;
                }
            }

            if (!swapped)
            {
                break;
            }
        }
    }
    
    public int Comparisons => _comparisons;
    public int SortComparisons => _sortComparisons;
    public int SortSwaps => _sortSwaps;
}