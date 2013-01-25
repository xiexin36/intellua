
class test2{};
class int{};
class TestClass{
	TestClass(int i);			///< constructor desc.
	TestClass TestMember1;		///< TestMember1 desc.
	void TestMethod(TestClass test,int a,int b,int c);		///< TestMethod desc.
	static void StaticMethod();
	virtual void VirtualMethod();
};

class TestDerived : TestClass{
	TestDerived();
	void DerivedMethod();
	virtual void VirtualMethod();
};
void GlobalFunction();
TestClass testInstance;