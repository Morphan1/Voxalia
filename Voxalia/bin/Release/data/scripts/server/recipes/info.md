Voxalia Recipes
---------------

Recipes are defined like so:

```
addrecipe strict ITEM_NAME
{
	result = RESULT;
}
```

Can do EG:

```
addrecipe strict <{item[ITEM_NAME].with_count[5]}>
{
	result = <{item[RESULT].with_count[2]}>;
}
```

Can result AIR to cause the crafting to not give a result.

Can get fancy:

```
addrecipe type|display <{item[stick].with_count[5]}>
{
	result = "<{var[USED_INPUT].get[1].with_count[1].with_display_name[Modded Stick]}>";
}
```

Valid options for the first argument:
- Strict: Exact match - every value must match!
- List|Of|Items: all the specified values are required, plus the count always. Accepted:
	- Type: The primary item type
	- Secondary: The secondary item type
	- Texture: The item texture
	- Model: The item model
	- Shared: The shared attributes
	- Local: The local attributes
	- Display: The display name
	- Description: The item description text
	- Color: The item's draw color
	- Datum: The item datum
	- Bound: Whether the item is bound
