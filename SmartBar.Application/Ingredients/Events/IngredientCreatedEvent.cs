using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBar.Application.Ingredients.Events;
public record IngredientCreatedEvent(Guid IngredientId, string Name);