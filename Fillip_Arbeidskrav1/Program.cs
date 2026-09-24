using System;
using System.IO;

namespace Fillip_Arbeidskrav1;

class Program
{
    static void Main(string[] args)
    {
        string phonebookPath = Path.Combine(AppContext.BaseDirectory, "data", "phonebook.csv");

        Phonebook phonebook;
        try
        { 
            phonebook = Phonebook.Load(phonebookPath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load phone book: {e.Message}");
            return;
        }
        
        Console.WriteLine("--- 1. Linear search, unsorted ---");
        Console.WriteLine($"{"Field",-12}{"target",-15}{"Matches",-10}{"Comparisons",-12}");
        
        RunLinearSearch(phonebook, Field.Lastname, "Bjerke");
        RunLinearSearch(phonebook, Field.Lastname, "Mathisen");
        RunLinearSearch(phonebook, Field.Lastname, "Husebø");
        RunLinearSearch(phonebook, Field.Mobile, "0000000");
        Console.WriteLine();

        Console.WriteLine("--- 2. Bubble sort ---");
        Console.WriteLine($"{"algorithm",-14}{"shape",-16}{"comparisons",-12}{"swaps",-10}");
        
        RunBubbleSort(phonebookPath, "As supplied", contacts => contacts);
        
        RunMergeSort(phonebookPath, "as-supplied", contacts => contacts);
        
        TestEdgeCases();
        TestAllFieldsAndOrders(phonebookPath);
        TestBinarySearch(phonebookPath);
    }

    static void RunLinearSearch(Phonebook phonebook, Field field, string target)
    {
        Contact[] results = phonebook.LinearSearch(field, target);
        Console.WriteLine($"{field,-12}{target,-15}{results.Length,-10}{phonebook.Comparisons,-12}");
    }

    static void RunBubbleSort(string phonebookPath, string shapeLabel, Func<Contact[], Contact[]> shapedFunc)
    {
        Phonebook phonebook;
        try
        { 
            phonebook = Phonebook.Load(phonebookPath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load phone book: {e.Message}");
            return;
        }
        
        Contact[] shaped = shapedFunc(phonebook.GetAll());
        Array.Copy(shaped, phonebook.GetAll(), shaped.Length);
        phonebook.BubbleSort(Field.Lastname, SortOrder.Ascending);

        Console.WriteLine($"{"BubbleSort",-14}{shapeLabel,-16}{phonebook.SortComparisons,-12}{phonebook.SortSwaps,-10}");
    }

    static void RunMergeSort(string phonebookPath, string shapeLabel, Func<Contact[], Contact[]> shapedFunc)
    {
        Phonebook phonebook;
        try
        {
            phonebook = Phonebook.Load(phonebookPath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load phone book: {e.Message}");
            return;
        }

        Contact[] shaped = shapedFunc(phonebook.GetAll());
        Array.Copy(shaped, phonebook.GetAll(), shaped.Length);

        phonebook.MergeSort(Field.Lastname, SortOrder.Ascending);

        Console.WriteLine($"{"MergeSort",-14}{shapeLabel,-16}{phonebook.SortComparisons,-12}{phonebook.SortMoves,-10}");
    }
     static void TestEdgeCases()
        {
            Console.WriteLine();
            Console.WriteLine("--- Edge case checks ---");
            
            Phonebook empty = Phonebook.FromContacts(Array.Empty<Contact>());
            empty.BubbleSort(Field.Lastname, SortOrder.Ascending);
            Console.WriteLine($"BubbleSort empty array survived: {empty.GetAll().Length == 0}");
            
            empty = Phonebook.FromContacts(Array.Empty<Contact>());
            empty.MergeSort(Field.Lastname, SortOrder.Ascending);
            Console.WriteLine($"MergeSort empty array survived: {empty.GetAll().Length == 0}");

            Contact single = new Contact("Ola", "Nordmann", "12345678", DateTime.Now, "Gate 1", "Oslo");
            
            Phonebook one = Phonebook.FromContacts(new[] { single });
            one.BubbleSort(Field.Lastname, SortOrder.Ascending);
            Console.WriteLine($"BubbleSort single-element survived: {one.GetAll().Length == 1 && one.GetAll()[0] == single}");
            
            one = Phonebook.FromContacts(new[] { single });
            one.MergeSort(Field.Lastname, SortOrder.Ascending);
            Console.WriteLine($"MergeSort single-element survived: {one.GetAll().Length == 1 && one.GetAll()[0] == single}");
        }

        static void TestAllFieldsAndOrders(string PhonebookPath)
        {
            Console.WriteLine();
            Console.WriteLine("--- Correctness across all fields and orders ---");

            foreach (Field field in Enum.GetValues<Field>())
            {
                foreach (SortOrder sortOrder in Enum.GetValues<SortOrder>())
                {
                    Phonebook pb1 = Phonebook.Load(PhonebookPath);
                    pb1.BubbleSort(field, sortOrder);
                    bool bubbleOk = VerifySorted(pb1.GetAll(), field, sortOrder);
                    
                    Phonebook pb2 = Phonebook.Load(PhonebookPath);
                    pb2.MergeSort(field, sortOrder);
                    bool mergeOk = VerifySorted(pb2.GetAll(), field, sortOrder);
                    
                    Console.WriteLine($"{field,-10}{sortOrder,-12}BubbleSort: {(bubbleOk ? "PASS" : "FAIL")}   MergeSort: {(mergeOk ? "PASS" : "FAIL")}");
                }
            }
        }
     
        static bool VerifySorted(Contact[] contacts, Field field, SortOrder sortOrder)
        {
            for (int i = 0; i < contacts.Length -1; i++)
            {
                string a = GetFieldValue(contacts[i], field);
                string b = GetFieldValue(contacts[i + 1], field);
                
                int result = string.Compare(a, b,  StringComparison.OrdinalIgnoreCase);
                
                if (sortOrder == SortOrder.Descending)
                {
                    result = -result;
                }

                if (result > 0)
                {
                    return false;
                }
            }
            return true;
        }

        static void TestBinarySearch(string phonebookPath)
        {
            Console.WriteLine();
            Console.WriteLine("--- 3. Binary search, sorted ---");
            Console.WriteLine($"{"#",-4}{"sorted by",-12}{"target",-15}{"result",-10}{"comparisons",-12}outcome");

            string anyMobile = "45101031";
            string absentMobileLow = "00000000";
            string absentMobileHigh = "9999999";
            string duplicateSurname = "Haugen";
            string absentSurname = "Husebø";
            string anyFirstName = "Astrid";
            
            //test 1 a real number in phonebook
            Phonebook pb1 = Phonebook.Load(phonebookPath);
            pb1.BubbleSort(Field.Mobile, SortOrder.Ascending);
            int result1 = pb1.BinarySearch(Field.Mobile, anyMobile);
            PrintTest(1, "Mobile", anyMobile, result1, pb1.SearchComparisons, result1 != -1);
            
            //test 2 mobile below lowest
            Phonebook pb2 = Phonebook.Load(phonebookPath);
            pb2.BubbleSort(Field.Mobile, SortOrder.Ascending);
            int result2 = pb2.BinarySearch(Field.Mobile, absentMobileLow);
            PrintTest(2, "Mobile", absentMobileLow, result2, pb2.SearchComparisons, result2 == -1);
            
            //test 3 Mobile above highest
            Phonebook pb3 = Phonebook.Load(phonebookPath);
            pb3.BubbleSort(Field.Mobile, SortOrder.Ascending);
            int result3 = pb3.BinarySearch(Field.Mobile, absentMobileHigh);
            PrintTest(3, "Mobile", absentMobileHigh, result3, pb3.SearchComparisons, result3 == -1);
            
            //test 4 Last name duplicate surname
            Phonebook pb4 = Phonebook.Load(phonebookPath);
            pb4.BubbleSort(Field.Lastname, SortOrder.Ascending);
            int result4 = pb4.BinarySearch(Field.Lastname, duplicateSurname);
            bool proven4 = ProveLowestIndex(pb4.GetAll(), result4, Field.Lastname);
            PrintTest(4, "LastName", duplicateSurname, result4, pb4.SearchComparisons, result4 != -1 && proven4);
            
            //test 5 Lastname absent
            Phonebook pb5 = Phonebook.Load(phonebookPath);
            pb5.BubbleSort(Field.Lastname, SortOrder.Ascending);
            int result5 = pb5.BinarySearch(Field.Lastname, absentSurname);
            PrintTest(5, "LastName", absentSurname, result5, pb5.SearchComparisons, result5 == -1);
            
            //test 6 first name proven
            Phonebook pb6 = Phonebook.Load(phonebookPath);
            pb6.BubbleSort(Field.Firstname, SortOrder.Ascending);
            int result6 = pb6.BinarySearch(Field.Firstname, anyFirstName);
            bool proven6 = ProveLowestIndex(pb6.GetAll(), result6, Field.Firstname);
            PrintTest(6, "FirstName", anyFirstName, result6, pb6.SearchComparisons, result6 != -1 && proven6);
            
            //Test 7 any field empty
            Phonebook empty = Phonebook.FromContacts(Array.Empty<Contact>());
            int result7 = empty.BinarySearch(Field.Lastname, "anything");
            PrintTest(7, "Any", "anything", result7, empty.SearchComparisons, result7 == -1);
            
            //test 8 any field single single
            Contact single = new Contact("Ola", "Nordmann", "12345678", DateTime.Now, "street 1", "Oslo");
            Phonebook one = Phonebook.FromContacts(new[] { single });
            int result8 = one.BinarySearch(Field.Lastname, "Nordmann");
            PrintTest(8, "Any", "Nordmann", result8, one.SearchComparisons, result8 == 0);
            
            //Phonebook pb9 = Phonebook.Load(phonebookPath);
           // pb9.BubbleSort(Field.Lastname, SortOrder.Ascending);
           // int result9 = pb9.BinarySearch(Field.Mobile, absentSurname);
           // PrintTest(9, "LastName", absentSurname, result5, pb5.SearchComparisons, result5 == -1);
        }
        
        static void PrintTest(int number, string sortedBy, string target, int result, int comparisons, bool passed)
        {
            Console.WriteLine($"{number,-4}{sortedBy,-12}{target,-15}{result,-10}{comparisons,-12}{(passed ? "PASS" : "FAIL")}");
        }

        static bool ProveLowestIndex(Contact[] contacts, int index, Field field)
        {
            if (index <= 0)
            {
                Console.WriteLine($"  index {index} is at or before array start, no earlier entry to check");
                return true;
            }
            
            string atIndex = GetFieldValue(contacts[index], field);
            string before = GetFieldValue(contacts[index - 1], field);
            
            Console.WriteLine($"  check: contacts[{index - 1}] = {before}, contacts[{index}] = {atIndex}, different: {before != atIndex}");
            return !string.Equals(before, atIndex, StringComparison.OrdinalIgnoreCase);
        }
        
        static string GetFieldValue(Contact contact, Field field)
        {
            return field switch
            {
                Field.Firstname => contact.FirstName,
                Field.Lastname => contact.LastName,
                Field.Mobile => contact.MobileNumber,
                _ => throw new ArgumentOutOfRangeException(nameof(field))
            };
        }
}