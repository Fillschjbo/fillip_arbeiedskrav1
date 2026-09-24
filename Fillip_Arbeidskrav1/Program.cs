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
       
        //only for test purposes
        RunBubbleSort(phonebookPath, "Allready sorted", contacts =>
            contacts.OrderBy(c => c.LastName, StringComparer.OrdinalIgnoreCase).ToArray());
        RunBubbleSort(phonebookPath, "Reverse sorted", contacts =>
            contacts.OrderByDescending(c => c.LastName, StringComparer.OrdinalIgnoreCase).ToArray());
        
        RunMergeSort(phonebookPath, "as-supplied", contacts => contacts);
        
        //only for test purposes
        RunMergeSort(phonebookPath, "already-sorted", contacts =>
            contacts.OrderBy(c => c.LastName, StringComparer.OrdinalIgnoreCase).ToArray());
        RunMergeSort(phonebookPath, "reverse-sorted", contacts =>
            contacts.OrderByDescending(c => c.LastName, StringComparer.OrdinalIgnoreCase).ToArray());
        
        TestEdgeCases();
        
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
        
        
        //test purposes only
        Contact[] result = phonebook.GetAll();
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"  {result[i].LastName}");
        }
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

        //test purposes only
        Contact[] result = phonebook.GetAll();
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"  {result[i].LastName}");
        }
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
}