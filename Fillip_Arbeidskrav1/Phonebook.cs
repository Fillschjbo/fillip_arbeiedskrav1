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
    
    /// <summary>
    /// Creates a Phonebook directly from an existing array of contacts, bypassing CSV loading.
    /// Intended for testing edge cases (empty array, single-element array) that Load cannot produce
    /// from the fixed 200-row phonebook.csv file.
    /// Time complexity: O(1). Space complexity: O(1) beyond the array reference passed in.
    /// </summary>
    /// <param name="contacts">The contacts to hold. Must not be null.</param>
    /// <returns>A new Phonebook wrapping the given array.</returns>
    public static Phonebook FromContacts(Contact[] contacts) => new Phonebook(contacts);
    
    /// <summary>
    /// Loads all contacts from a CSV file into a new Phonebook. Expects a header row followed by
    /// comma-separated rows in the order FirstName,LastName,Mobile,Birthday,Street,City.
    /// Time complexity: O(n), where n is the number of rows in the file.
    /// Space complexity: O(n), for the resulting Contact array.
    /// </summary>
    /// <param name="phonebook">Path to the CSV file to load.</param>
    /// <returns>A new Phonebook containing every row from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the containing directory does not exist.</exception>
    /// <exception cref="FormatException">Thrown when the file has no data rows, a row has the wrong number of fields, or a birthday cannot be parsed.</exception>
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
    
    /// <summary>
    /// Returns the value of the given field for a contact, as a string, so that searching and
    /// sorting can operate generically across FirstName, LastName, and Mobile without duplicating
    /// logic per field.
    /// Time complexity: O(1). Space complexity: O(1).
    /// </summary>
    /// <param name="contact">The contact to read from.</param>
    /// <param name="field">Which property to extract.</param>
    /// <returns>The string value of the requested field.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when field is not a recognised value.</exception>
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
    
    /// <summary>
    /// Compares two contacts on the given field, honoring ascending or descending order, and
    /// increments the sort comparison counter. Used by both sorting algorithms so that comparison
    /// counting logic exists in only one place.
    /// Time complexity: O(1). Space complexity: O(1).
    /// </summary>
    /// <param name="a">The first contact.</param>
    /// <param name="b">The second contact.</param>
    /// <param name="field">Which field to compare on.</param>
    /// <param name="order">Ascending or descending.</param>
    /// <returns>Negative if a comes before b, zero if equal, positive if a comes after b, given the requested order.</returns>
    private int CompareContacts(Contact a, Contact b, Field field, SortOrder order)
    {
        _sortComparisons++;
        int result = string.Compare(Key(a, field), Key(b, field), StringComparison.OrdinalIgnoreCase);
        return order == SortOrder.Ascending ? result : -result;

    }
    
    /// <summary>
    /// Searches the array from the start, returning every contact whose given field matches the
    /// target exactly (case-insensitive). Works on unsorted data.
    /// Time complexity: O(n) in all cases, since every contact must be visited to guarantee all
    /// matches are found. Space complexity: O(n) worst case, if every contact matches.
    /// </summary>
    /// <param name="field">Which field to search on.</param>
    /// <param name="target">The value to match exactly, ignoring case.</param>
    /// <returns>Every matching contact, in original order, or an empty array if none match. Never null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target is null.</exception>
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
    
    /// <summary>
    /// Returns the underlying contact array, for inspection and testing.
    /// Time complexity: O(1). Space complexity: O(1) (returns the existing array, not a copy).
    /// </summary>
    /// <returns>The live array of contacts held by this Phonebook.</returns>
    public Contact[] GetAll() => _contacts;
    
    /// <summary>
    /// Sorts the array in place on the given field and order, using Bubble Sort with an
    /// early-exit optimisation that stops as soon as a full pass makes no swaps.
    /// Time complexity: O(n) best case (already sorted), O(n^2) average and worst case.
    /// Space complexity: O(1), sorts in place.
    /// </summary>
    /// <param name="field">Which field to sort on.</param>
    /// <param name="order">Ascending or descending.</param>
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
    
    /// <summary>
    /// Sorts the array on the given field and order using Merge Sort.
    /// Time complexity: O(n log n) in all cases (best, average, and worst).
    /// Space complexity: O(n), for the temporary buffers allocated during each merge step.
    /// Unlike BubbleSort, this is not a pure in-place sort: merging two sorted halves requires
    /// copying them into temporary arrays before writing the merged result back.
    /// </summary>
    /// <param name="field">Which field to sort on.</param>
    /// <param name="order">Ascending or descending.</param>
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
    
    /// <summary>
    /// Recursively splits the range [left, right] in half, sorts each half, then merges them.
    /// Time complexity: O(n log n). Space complexity: O(log n) for the recursion stack,
    /// plus the O(n) temporary buffers allocated across all Merge calls.
    /// </summary>
    /// <param name="left">Start index of the range to sort, inclusive.</param>
    /// <param name="right">End index of the range to sort, inclusive.</param>
    /// <param name="field">Which field to sort on.</param>
    /// <param name="order">Ascending or descending.</param>
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
    
    /// <summary>
    /// Merges two already-sorted sub-ranges, [left, mid] and [mid+1, right], back into the array
    /// in sorted order, using temporary buffers to hold each half during the merge.
    /// Time complexity: O(k), where k is the combined length of the two sub-ranges.
    /// Space complexity: O(k), for the two temporary arrays.
    /// </summary>
    /// <param name="left">Start index of the left sub-range.</param>
    /// <param name="mid">End index of the left sub-range; the right sub-range starts at mid+1.</param>
    /// <param name="right">End index of the right sub-range.</param>
    /// <param name="field">Which field to compare on.</param>
    /// <param name="order">Ascending or descending.</param>
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
    
    /// <summary>
    /// Searches the array, which must already be sorted ascending by the given field, for the
    /// first (lowest-index) occurrence of target. Serves all three fields through the same method.
    /// Time complexity: O(log n) for the halving search itself, plus O(n) for the precondition
    /// check that verifies the array is actually sorted by the given field before searching.
    /// Space complexity: O(1).
    /// </summary>
    /// <param name="field">Which field the array must be sorted by, and to search on.</param>
    /// <param name="target">The value to find, matched exactly and case-insensitively.</param>
    /// <returns>The lowest index at which target occurs, or -1 if it is not present.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the array is not sorted ascending by field.</exception>
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
    
    /// <summary>
    /// Checks whether the array is sorted in ascending order by the given field.
    /// Used as a precondition check before BinarySearch runs.
    /// Time complexity: O(n). Space complexity: O(1).
    /// </summary>
    /// <param name="field">Which field to check ordering on.</param>
    /// <returns>True if every element is less than or equal to the next, false otherwise.</returns>
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
    
    /// <summary>Number of comparisons made by the most recent LinearSearch call.</summary>
    public int Comparisons => _comparisons;
    
    /// <summary>Number of comparisons made by the most recent BubbleSort or MergeSort call.</summary>
    public int SortComparisons => _sortComparisons;
    
    /// <summary>Number of swaps made by the most recent BubbleSort call.</summary>
    public int SortSwaps => _sortSwaps;
    
    /// <summary>Number of element moves made by the most recent MergeSort call.</summary>
    public int SortMoves => _sortMoves;
    
    /// <summary>Number of comparisons made by the most recent BinarySearch call (excludes the O(n) precondition check).</summary>
    public int SearchComparisons => _searchComparisons;
}