#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>

extern int minsk_rand(int);
extern int start_timer();
extern int stop_timer();
extern void fillArrayWithRandomNumbers(long long* numbers, long long numbersCount);
void printNumbers(long long* numbers, long long numbersCount);

void exch(long long* a, long long x, long long y)
{
    long long tmp = a[x];
    a[x] = a[y];
    a[y] = tmp;
}

long long partition(long long* a, long long l, long long r)
{
    long long i = l-1;
    long long j = r;
    long long v = a[r];

    while (true)
    {
        while (a[++i] < v) { }
        while (v < a[--j])
        {
            if (j == l)
                break;
        }
        if (i >= j)
            break;

        exch(a, i, j);
    }
    exch(a, i, r);
    return i;
}

void quicksort(long long* a, long long l, long long r)
{
    if (r <= l)
        return;
    long long i = partition(a, l, r);
    quicksort(a, l, i - 1);
    quicksort(a, i + 1, r);
}

int main()
{
    long numbersCount = 10000000;
    //printf("numbers count:\n");
    //scanf("%ld", &numbersCount);
    long long *numbers = (long long*)malloc(numbersCount * sizeof(long long)); 
    fillArrayWithRandomNumbers(numbers, numbersCount);
    start_timer();
    quicksort(numbers, 0, numbersCount - 1);
    stop_timer();

    //printNumbers(numbers, numbersCount);

    return 1;
}