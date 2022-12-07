using System;
using Verse;
using RimWorld;


namespace List_Everything
{
  //automated ExposeData + Clone 
  public abstract class ListFilterWithOption<T> : ListFilter
	{
		// selection
		private T _sel;
		protected string refName;// if UsesRefName,  = SaveLoadXmlConstants.IsNullAttributeName;
		private int _extraOption; //0 meaning use _sel, what 1+ means is defined in subclass

		// A subclass with extra fields needs to override ExposeData and Clone to copy them

		public string selectionError; // Probably set on load when selection is invalid (missing mod?)
		public override string DisableReason => base.DisableReason ?? selectionError;

		// would like this to be T const * sel;
		public T sel
		{
			get => _sel;
			set
			{
				_sel = value;
				_extraOption = 0;
				selectionError = null;
				if (UsesRefName) refName = MakeRefName();
				PostSelected();
			}
		}

		// A subclass should often set sel in the constructor
		// which will call the property setter above
		// If the default is null, and there's no PostSelected to do,
		// then it's fine to skip defining a constructor
		protected ListFilterWithOption()
		{
			if (UsesRefName)
				refName = SaveLoadXmlConstants.IsNullAttributeName;
		}
		protected virtual void PostSelected()
		{
			// A subclass with fields whose validity depends on the selection should override this
			// Most common usage is to set a default value that is valid for the selection
			// e.g. the skill filter has a range 0-20, but that's valid for all skills, so no need to reset here
			// e.g. the hediff filter has a range too, but that depends on the selected hediff, so the selected range needs to be set here
		}

		// This method works double duty:
		// Both telling if Sel can be set to null, and the string to show for null selection
		public virtual string NullOption() => null;

		protected int extraOption
		{
			get => _extraOption;
			set
			{
				_extraOption = value;
				_sel = default;
				selectionError = null;
				refName = null;
			}
		}

		//Okay, so, references.
		//A simple filter e.g. string search is usable everywhere.
		//In-game, as an alert, as a saved filter to load in, saved to file to load into another game, etc.
		//ExposeData and Clone can just copy T sel, because a string is the same everywhere.
		//But a filter that references in-game things can't be used universally
		//When such a filter is run in-game, it does of course set 'sel' and reference it like normal
		//But when such a filter is saved, it cannot be bound to an instance
		//So ExposeData saves and loads 'string refName' instead of the 'T sel'
		//When showing that filter as an option to load, that's fine, sel isn't set but refName is.
		//When the filter is copied, loaded or saved in any way, it is cloned with Clone(), which will copy refName but not sel
		//When loading or copying into a map, whoever called Clone will also call ResolveReference(Map) to bind to that map
		//(even if a copy ends up referencing the same thing, the reference is re-resolved for simplicity's sake)

		//TL;DR there are two 'modes' a ListFilter can be: active or inactive.
		//When active, it's bound to a map, ready to do actual filtering based on sel
		//When inactive, it's in storage - it only knows the name of sel
		//When loading an inactive filter, the refname+map are used to find and set sel
		//When saving an active filter, just refname is saved
		//When copinying an active filter, refname is copied and sel is found again
		//(Of course if you don't use refname, the filter just copies sel around)

		protected readonly static bool IsDef = typeof(Def).IsAssignableFrom(typeof(T));
		protected readonly static bool IsRef = typeof(ILoadReferenceable).IsAssignableFrom(typeof(T));
		protected readonly static bool IsEnum = typeof(T).IsEnum;

		public virtual bool UsesRefName => IsRef || IsDef;
		protected virtual string MakeRefName() => sel?.ToString() ?? SaveLoadXmlConstants.IsNullAttributeName;

		// Subclasses where UsesRefName==true need to implement ResolveReference()
		// (unless it's just a Def)
		// return matching object based on refName (refName will not be "null")
		// returning null produces a selection error and the filter will be disabled
		protected virtual T ResolveReference(Map map)
		{
			if (IsDef)
			{
				//Scribe_Defs.Look doesn't work since it needs the subtype of "Def" and T isn't boxed to be a Def so DefFromNodeUnsafe instead
				//_sel = ScribeExtractor.DefFromNodeUnsafe<T>(Scribe.loader.curXmlParent["sel"]);

				//DefFromNodeUnsafe also doesn't work since it logs errors - so here's custom code copied to remove the logging:

				return (T)(object)GenDefDatabase.GetDefSilentFail(typeof(T), refName, false);
			}

			throw new NotImplementedException();
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref _extraOption, "ex");
			if (_extraOption > 0)
			{
				if (Scribe.mode == LoadSaveMode.LoadingVars)
					extraOption = _extraOption; // property setter to set other fields null

				// No need to worry about sel or refname, we're done!
				return;
			}

			//Oh Jesus T can be anything but Scribe doesn't like that much flexibility so here we are:
			//(avoid using property 'sel' so it doesn't MakeRefName())
			if (UsesRefName)
			{
				// Of course between games you can't get references so just save by name should be good enough
				// (even if it's from the same game, it can still resolve the reference all the same)

				// Saving a null refName saves "IsNull"
				Scribe_Values.Look(ref refName, "refName");

				// ResolveReferences() will be called when loaded onto a map for actual use
			}
			else if (typeof(IExposable).IsAssignableFrom(typeof(T)))
			{
				//This might just be to handle ListFilterSelection
				Scribe_Deep.Look(ref _sel, "sel");
			}
			else
				Scribe_Values.Look(ref _sel, "sel");
		}
		public override ListFilter Clone()
		{
			ListFilterWithOption<T> clone = (ListFilterWithOption<T>)base.Clone();

			clone.extraOption = extraOption;
			if (extraOption > 0)
				return clone;

			if (UsesRefName)
				clone.refName = refName;
			else
				clone._sel = _sel;  //todo handle if sel needs to be deep-copied. Perhaps sel should be T const * sel...

			return clone;
		}
		public override void DoResolveReference(Map map)
		{
			if (!UsesRefName || extraOption > 0) return;

			if (refName == SaveLoadXmlConstants.IsNullAttributeName)
			{
				_sel = default; //can't use null because generic T isn't bound as reftype
			}
			else
			{
				_sel = ResolveReference(map);

				if (_sel == null)
				{
					selectionError = $"Missing {def.LabelCap}: {refName}?";
					Messages.Message("TD.TriedToLoad0FilterNamed1ButCouldNotBeFound".Translate(def.LabelCap, refName), MessageTypeDefOf.RejectInput);
				}
			}
		}
	}
}
