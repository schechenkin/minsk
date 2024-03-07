function main() {
    var x = 0
    var a = 42
    var b = 3
    var c = 43
    var d = 0

    var i = 1

    do
    {
        a = 2
        c = 3

        if(a < c)
        {
            b = 4
            c = 5
            d = 6
        }
        else
        {
            a = 7
            d = 8

            if(a <= d)
            {
                d = 9
            }
            else
            {
                c = 10
            }
            b = 11
        }

        var y = a + b
        var z = c + d
        i = i + 1
    }
    while(i <= 100)

    print(i)
}

/*
function main()
{
    var x = 0
    let a0 = 42
    let b0 = 3
    let c0 = 43
    let d0 = 0
    let i0 = 1
    a1 = a0
    b1 = b0
    c1 = c0
    d1 = d0
    i1 = i0
Label1:
    a2 = 2
    c2 = 3
    goto Label2 unless a2 < c2
    b2 = 4
    c3 = 5
    d2 = 6
    a3 = a2
    d3 = d2
    goto Label3
Label2:
    a4 = 7
    d4 = 8
    goto Label4 unless a4 <= d4
    d5 = 9
    goto Label5
Label4:
    c4 = 10
Label5:
    b3 = 11
    a3 = a4
    d3 = d4
Label3:
    var y = a3 + b1
    var z = c2 + d3
    i2 = i1 + 1
    goto Label9 if i2 <= 100
    print(string(i2))
    return
Label9:
    a1 = a3
    b1 = b1
    c1 = c2
    d1 = d3
    i1 = i2
    goto Label1
}

*/