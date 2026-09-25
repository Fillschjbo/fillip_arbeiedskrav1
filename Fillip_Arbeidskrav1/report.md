# Searching and Sorting: Report

| |                             |
|---|-----------------------------|
| Name | Fillip Husebø               |
| Date | 24 September 2026           |
| Data | phonebook.csv, 200 contacts |

---

## 1. Searching unsorted data

| Field | Target | Case | Matches | Comparisons |
|---|---|---|---|---|
| LastName | Bjerke | first record (best case) | 9 | 200 |
| LastName | Hansen | last record (worst case) | 7 | 200 |
| LastName | Husebø | absent value | 0 | 200 |
| Mobile | 0000000 | absent value | 0 | 200 |

When reviewing the data we can see that a linear search over unsorted data gives us 200 comparisons (n) in all cases. A linear search across unsorted data must return every match, meaning it has to go through the entire dataset and cannot exit early. This is why we see both best and worst cases use the same amount of comparisons. A linear search can in theory give us a best case of O(1) and in worst cases O(n), but since the assignment asks us to return every match it gives us O(n) in all scenarios. This means that a search for nine matches and a search for zero matches have the same cost.

## 2. Sorting

| Algorithm | Input shape | Comparisons | Swaps or moves |
|---|---|---|---|
| Bubble Sort | as supplied | 36815 | 9494 |
| Bubble Sort | already sorted | 199 | 0 |
| Bubble Sort | reverse sorted | 39601 | 19411 |
| Merge Sort | as supplied | 1282 | 1544 |
| Merge Sort | already sorted | 812 | 1544 |
| Merge Sort | reverse sorted | 890 | 1544 |

When testing bubble sort and merge sort on 3 different shapes of data: as-supplied, already sorted and reverse sorted, we can see that there is a clear winner. Merge sort does significantly less work across the board. It is worth noting, however that bubble sort is sginificantly faster with already sorted data with only 199 comparisons and 0 swaps. Bubble sort is significantly hurt, doing 36815 comparisons and 9494 swaps on as-supplied and 39601 comparisons and 1941 1swaps on reverse sorted datasets. This all comes down to bubble sorts early exit mechanism, which lets it finish in one pass on already sorted data. Merge Sort's moves stay at exactly 1544 because every level of recursion copies every element into the merged output once regardless of order, so shape changes only how many comparisons each merge needs. In theory, Bubble Sort with an early-exit check is O(n) in the best case and O(n²) in the average and worst cases, while Merge Sort is O(n log n) in all three. My counts agree with this. With n = 200, Bubble Sort's sorted case is n − 1 = 199 comparisons and its reverse case is (n − 1)² = 39601, while Merge Sort's comparisons stay between 812 and 1282, below n log₂ n ≈ 1529.

## 3. Searching sorted data

| Field | Target | Result | Comparisons |
|---|---|---|---|
| LastName | Haugen | 77 | 8 |
| LastName | Husebø | -1 | 8 |
| Mobile | 45101031 | 52 | 8 |
| FirstName | Astrid | 17 | 8 |

Linear search on the same targets, for comparison:

| Target | Comparisons (linear) | Comparisons (binary) |
|---|---|---|
| Haugen | 200 | 8 |
| Husebø | 200 | 8 |
| 45101031 | 200 | 8 |
| Astrid | 200 | 8 |

My binary search took about 8 comparisons per lookup, which matches log₂(200) ≈ 7.64 rounded up: you can't do a fraction of a comparison, and the worst case for binary search is ⌊log₂ n⌋ + 1 = 8. To guarantee the first occurrence of a duplicated surname, the search doesn't stop at the first match it finds. It records that index as the best answer so far, then keeps narrowing to the left half, so any earlier match overwrites it and the last one recorded is the first occurrence. Each binary search saves about 200 − 8 = 192 comparisons over a linear search, so the break-even point is the sorting cost divided by 192. With Bubble Sort's 36815 comparisons that means 192 searches (36815 + 192 × 8 = 38351 against 192 × 200 = 38400). With Merge Sort's 1282 comparisons it takes only 7 searches (1282 + 7 × 8 = 1338 against 1,400), which shows that a fast sort makes sorting first pay off almost immediately.

## 4. Insight
 
The most useful thing my numbers taught me is that an algorithm's best case can be misleading, and what matters is how it behaves on the data you actually expect and how often you'll rely on the result. Bubble Sort looked brilliant on already-sorted input, finishing in 199 comparisons against Merge Sort's 812. But on the list as supplied it needed 36815 comparisons, almost 29 times Merge Sort's 1282, and on reverse-sorted input it climbed to 39601. That gap carried straight through to searching: with Merge Sort, sorting first paid off after just 7 binary searches, but with Bubble Sort it took 192, almost as many searches as there are records. So the choice of sorting algorithm decided whether sorting-then-searching was worth doing at all. From now on I would pick an algorithm by its typical and worst-case cost and by how the output will be used, not by how well it does when the data happens to be kind.