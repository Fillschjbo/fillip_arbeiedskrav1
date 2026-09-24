namespace Fillip_Arbeidskrav1;

public class Phonebook
{
    private int _comparisons;
    private int _sortComparisons;
    private int _sortSwaps;
    private int _sortMoves;
    private int _searchComparisons;

    private readonly Contact[] _contacts;

    private Phonebook(Contact[] contacts)
    {
        _contacts = contacts;
    }

    public static Phonebook FromContacts(Contact[] contacts) => new Phonebook(contacts);

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

    public void MergeSort(Field field, SortOrder order)
    {
        _sortComparisons = 0;
        _sortMoves = 0;

        if (_contacts.Length < 2)
        {
            return;
        }
        MergeSortRecursive(0, _contacts.Length - 1, field, order);
    }

    private void MergeSortRecursive(int left, int right, Field field, SortOrder order)
    {
        if (left >= right)
        {
            return;
        }
        
        int mid = left + (right - left) / 2;
        
        MergeSortRecursive(left, mid, field, order);
        MergeSortRecursive(mid + 1, right, field, order);
        Merge(left, mid, right, field, order);
    }

    private void Merge(int left, int mid, int right, Field field, SortOrder order)
    { 
        int leftLength = mid - left + 1;
        int rightLength = right -mid;

        var leftTemp = new Contact[leftLength];
        var rightTemp = new Contact[rightLength];
        
        Array.Copy(_contacts, left, leftTemp, 0, leftLength);
        Array.Copy(_contacts, mid + 1, rightTemp, 0, rightLength);

        int i = 0, j = 0, k = left;

        while (i < leftLength && j < rightLength)
        {
            if (CompareContacts(leftTemp[i], rightTemp[j], field, order) <= 0)
            {
                _contacts[k]  = leftTemp[i];
                i++;
            }
            else
            {
                _contacts[k] = rightTemp[j];
                j++;
            }
            _sortMoves++;
            k++;
        }

        while (i < leftLength)
        {
            _contacts[k] = leftTemp[i];
            i++; k++;
            _sortMoves++;
        }

        while (j < rightLength)
        {
            _contacts[k] = rightTemp[j];
            j++; k++;
            _sortMoves++;
        }
    }

    public int BinarySearch (Field field, string target)
    {
        if (target == null)
            { 
                throw new ArgumentNullException(nameof(target));
            }

        if (!IsSortedBy(field))
        {
            throw new InvalidOperationException($"BinarySearch precondition violated: array is not sorted ascending by {field}.");
        }
        
        _searchComparisons = 0;

        int left = 0;
        int right = _contacts.Length - 1;
        int resultIndex = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            
            _searchComparisons++;
            int comparison = string.Compare(Key(_contacts[mid],field), target, StringComparison.OrdinalIgnoreCase);

            if (comparison == 0)
            {
                resultIndex = mid;
                right = mid - 1;
            }
            else if (comparison < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        
        return resultIndex;
    }

    private bool IsSortedBy(Field field)
    {
        for (int i = 0; i < _contacts.Length - 1; i++)
        {
            if (string.Compare(Key(_contacts[i], field), Key(_contacts[i + 1], field), StringComparison.OrdinalIgnoreCase) > 0)
            {
                return false;
            }
        }
        return true;
    }

public int Comparisons => _comparisons;
    public int SortComparisons => _sortComparisons;
    public int SortSwaps => _sortSwaps;
    public int SortMoves => _sortMoves;
    public int SearchComparisons => _searchComparisons;
}