# Annotations #

Intellua search the lua text for assignments to deduce type of objects, but sometimes assignment is not avaliable(function parameter type) or can not be made (cause code behavior to change).

Annotations are special kind of Lua comment that starts with a '@'
```lua

--@ a = Vector()
--[[@ b = Vector()

]]
```

Lua interpreter will treat is as comment, but Intellua will act as if it is normal lua code.

Annotations can be used to assign type even if there are no constructors

```lua

--@ c = Vector
```

# Example #

```lua

function dist(p1,p2)
--@ p1 = Vector; p2 = Vector;
return (p2-p1):length()
end
```