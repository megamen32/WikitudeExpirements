
using System;
using ExternalAnnotationsGenerator;
using static ExternalAnnotationsGenerator.Annotations;
using UnityEngine;
namespace DefaultNamespace
{
	public class UnityExternalAnnotations
	{

	#region Methods

	#endregion

	#region Data

	#region Options

		void Main()
		{
			var annotator = Annotator.Create();
		//	annotator.Annotate((component) =>component. NotNull<Component>());

			annotator.Annotate<Component>((type)=>type.Annotate(component=>component.transform==NotNull<Transform>()));
			annotator.Annotate<Component>((type)=>type.Annotate(component=>component.gameObject==NotNull<GameObject>()));



		}

	#endregion

	#region States

	#endregion

	#region Properties

	#endregion

	#endregion

	}
}
