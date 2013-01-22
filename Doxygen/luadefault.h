class __LUA_MATH{
	/** \brief Returns the absolute value of x. 
	
	*/
	double abs(double x);

	


	
	/** \brief Returns the arc tangent of y/x (in radians), but uses the signs of both parameters to find the quadrant of the result. (It also handles correctly the case of x being zero.) 

	*/
	double atan2(double y,double x);
};

__LUA_MATH math;