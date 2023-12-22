enum Color { Black, White }

function main()
{
    print("What's your name, sir?")
    let name = input()
    print("Hello " + name + "!")

    print("Color is " + string(getColor()))
}

function getColor() : Color
{
    return Color.White
}
