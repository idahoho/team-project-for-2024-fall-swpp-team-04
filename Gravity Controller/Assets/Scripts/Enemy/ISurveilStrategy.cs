using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISurveilStrategy
{
	// Draw a route to repeat, according to x. (of period x=1)
	// Note: every route must be counter-clockwise
	public Vector2 Route(float x);
	// Scale the shape, horizontally and vertically
	public void SetScale(float horizontal, float vertical);
}

public class LinearSurveil : ISurveilStrategy
{
	private float _a = 1;
	public Vector2 Route(float x)
	{
		return new Vector2(_a * Mathf.Sin(x * (2 * Mathf.PI)), 0);
	}

	public void SetScale(float horizontal, float vertical)
	{
		_a = horizontal;
	}
}

public class CircularSurveil : ISurveilStrategy
{
	private float _r = 1;
	public Vector2 Route(float x)
	{
		return new Vector2(_r * Mathf.Cos(x * (2 * Mathf.PI)), _r * Mathf.Sin(x * (2 * Mathf.PI)));
	}

	public void SetScale(float horizontal, float vertical)
	{
		_r = horizontal;
	}
}

public class RectangularSurveil : ISurveilStrategy
{
	private float _a = 1;
	private float _b = 1;
	public Vector2 Route(float x)
	{
		/* period: x = 1
		 * 
		 *  - cos:
		 * |
		 * |¢Ù    ¢Ö¢Ù    ¢Ö
		 * |  ¢Ù¢Ö    ¢Ù¢Ö
		 * |
		 * 
		 *  - sin:
		 * |
		 * |¢Ö¢Ù    ¢Ö¢Ù
		 * |    ¢Ù¢Ö    ¢Ù¢Ö
		 * |
		 * 
		 *  - cos2: (with double y-scale)
		 * |¢Ù            ¢Ö
		 * |  ¢Ù        ¢Ö
		 * |    ¢Ù    ¢Ö    
		 * |      ¢Ù¢Ö
		 * 
		 *  - sin2: (with double y-scale)
		 * |  ¢Ö¢Ù          
		 * |¢Ö    ¢Ù        
		 * |        ¢Ù    ¢Ö    
		 * |          ¢Ù¢Ö
		 * 
		 * [Rectangle]
		 * B --- A
		 * |     |
		 * |     |
		 * C --- D
		 * A: x = 1/8
		 * B: x = 3/8
		 * C: x = 5/8
		 * D: x = 7/8
		 * 
		 * D -> A: -1/8 < x < 1/8, cos > 0, cos2 > 0
		 * A -> B:  1/8 < x < 3/8, cos < 0, sin2 > 0
		 * B -> C:  3/8 < x < 5/8, cos > 0, cos2 < 0
		 * C -> D:  5/8 < x < 7/8, cos < 0, sin2 < 0
		 */

		float cos = Mathf.Cos(x * (4 * Mathf.PI));
		float sin = Mathf.Sin(x * (4 * Mathf.PI));
		float cos2 = Mathf.Cos(x * (2 * Mathf.PI));
		float sin2 = Mathf.Sin(x * (2 * Mathf.PI));
		if (cos > 0)
		{
			// time to move vertically
			//return (cos2 > 0 ? 1 : -1) * new Vector2(_a, sin * _b);
			return (cos2 > 0 ? 1 : -1) * new Vector2(_a, sin * _b * 0.64f);
			// Note: 0.64f stands for 2/pi, the average of sin for a half-period (multiply this factor to obtain pseudo-circular movement)
		}
		else
		{
			// time to move horizontally
			//return (sin2 > 0 ? 1 : -1) * new Vector2(sin * _a, _b);
			return (sin2 > 0 ? 1 : -1) * new Vector2(sin * _a, _b * 0.64f);
		}
	}

	public void SetScale(float horizontal, float vertical)
	{
		_a = horizontal;
		_b = vertical;
	}
}

public class CylindricalConverter
{
	public static Vector3 Plane2Cylinder(Vector2 v)
	{
		// projects the plane onto the cylinder of radius 1
		// keeps the y-coordinate
		// (0,0) |-> (0,0,1)
		return new Vector3(-Mathf.Sin(v.x), v.y, Mathf.Cos(v.x));
	}

	public static Vector3 Cylinder2Plane(Vector3 v)
	{
		var cylindrical = new Vector2(v.x,v.z).normalized;
		return new Vector3(Mathf.Asin(- cylindrical.x), v.y);
	}
}