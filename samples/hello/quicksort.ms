function main()
{   
    print("numbers count:")
    let numbersCount : int = input()
    print("")
    var numbers: int[] = array(numbersCount)

    fillArrayWithRandomNumbers(numbers, numbersCount)
    start_timer()
    quicksort(numbers, 0, numbersCount - 1)
    stop_timer()
    printNumbers(numbers, numbersCount)
}

function quicksort(a: int[], l: int, r: int)
{
    if r <= l
        return
    var i = partition(a, l, r)
    quicksort(a, l, i - 1)
    quicksort(a, i + 1, r)
}

function partition(a: int[], l: int, r: int) : int
{
    var i = l-1
    var j = r
    var v = a[r]

    while true
    {
        while a[+i] < v { }
        while v < a[-j]
        {
            if j == l
                break
        }
        if i >= j
            break

        exch(a, i, j)
    }
    exch(a, i, r)
    return i
}

function fillArrayWithRandomNumbers(numbers: int[], count: int)
{
    for i = 0 to count-1
    {
        numbers[i] = rnd(100000)
    }
}

function printNumbers(numbers: int[], count: int)
{
    for i = 0 to count-1
    {
        print(numbers[i])
    }
}

function exch(a: int[], x: int, y: int)
{
    var tmp = a[x]
    a[x] = a[y]
    a[y] = tmp
}
