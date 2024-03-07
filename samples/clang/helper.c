#include <stdbool.h>
#include <stdio.h>

extern int minsk_rand(int);

void fillArrayWithRandomNumbers(long long* numbers, long long numbersCount)
{
    for(long long i = 0; i < numbersCount; i++)
    {
        numbers[i] = minsk_rand(10000000);
    }
}

void printNumbers(long long* numbers, long long numbersCount)
{
    for(long long i = 0; i < numbersCount; i++)
    {
        printf("%lld ", numbers[i]);
    }
}
