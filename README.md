# plugin-expression-pov
POV to create a parser of a grammar that we use internally in DP

## Situation
Currently we are using a very simple rule to determine if showing a plugin based in some tags.

For example, if we want show a plugin if `Doctors in Poland that are Vip and Plus` we should define this rule: `PL.DOCTOR.VIP,PL.DOCTOR.PLUS`

then based on that we will decide if showing/hiding a plugin based in a series of tags, for example:

- tags: `DOCTOR,PL,VIP,DENTIST,..` -> `true`
- tags: `DOCTOR,PL,STARTER` -> `false`

For defining the rules we can have:

- `*` -> Show for all
- `~` -> Hide for all
- `!` -> Should not include
- `,` -> OR
- `.` -> AND

> Just one note, if we have `!` that will take priority over the rest of parts. 
> 
> For example: `DOCTOR.PL,!DOCTOR.PL.DENTIST` the part `!DOCTOR.PL.DENTIST` will take more priority over `DOCTOR.PL` even if are joined by `OR`.

We cannot use parenthesis `()`. So it is hard of create rules. For example if we want a rule for:

- `Doctors in Poland that are Starters, Vip and Plus, but not for Dentist or Physio.` the rule can be very large: `PL.DOCTOR.STARTER,PL.DOCTOR.VIP,PL.DOCTOR.PLUS,!PL.DOCTOR.DENTIST,!PL.DOCTOR.PHYSIO`

## Propose

Create a new evaluation mechanism, more flexible and more powerful. For example if we include parenthesis to prioritize expressions we can do a lot of things. For example:

The previous rule: `Doctors in Poland that are Starters, Vip and Plus, but not for Dentist or Physio.` can be created like: `PL.DOCTOR.(STARTER,VIP,PLUS).!(DENTIST,PHYSIO)`

Weight simpler!!!

## Tasks

- Create a new Parser that will read the new grammar
- Then we will get a AST node
- Using this new AST we can evaluate a set of tags and will return `true` or `false`

### Grammar
We have defined the grammar (very simplified):

```
OR -> AND[,OR ]*

AND -> TERM[.AND]*

TERM -> WORD | !TERM | (OR) | * | ~
```

### Parser

We have created the new parser class: `ExpressionParser`. For example:

```c#
    var parser = new ExpressionParser("PL.DOCTOR.(STARTER,VIP,PLUS).!(DENTIST,PHYSIO)");
    var parseResult = parser.Parse(out NodeBase ast);
    var result = ast.Evaluate(["DOCTOR","PL","VIP","DENTIST"])); // will return false since it is a dentist
```

### Tested

This has being heavily tested. We can check the tests in order to find more use cases.

## Result

Done...