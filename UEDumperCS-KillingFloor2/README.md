# KillingFloor2D

<details>
  <summary>Extra</summary>
  
 ```cpp
// GObjects: [+0x233] E8 [....] E8 .... 4D 85 E4
//   GNames: [+0x7a ] E8 [....] 48 83 CB FF 45 85 F6

template<class T>
struct TArray
{
  TArray()
  {
    Data  = nullptr;
    Count = Max = 0;
  }

  size_t Num() const
  {
    return Num;
  }

  T& operator[](size_t i)
  {
    return Data[i];
  };

  const T& operator[](size_t i) const
  {
    return Data[i];
  };

  bool IsValidIndex(size_t i) const
  {
    return i < Num();
  }

private:
  T*      Data;
  int32_t Num;
  int32_t Max;
};

struct FQWord
{
  int32_t A;
  int32_t B;
};

struct FName
{
  int32_t Index;
  int32_t Number;
}

class FNameEntry
{
public:
  char    pad_0000[8];
  int32_t Index;
  char    pad_000C[8];
  char    Name[1024];
};

class UObject
{
public:
  void*    VTableObject;
  char     pad_0008[48];
  int32_t  ObjectInternalInteger;
  int32_t  NetIndex;
  UObject* Outer;
  FName    Name;
  UObject* Inner;
  UObject* ObjectArchetype;
};
 ```
</details>

![IMG](/.github/resources/kf2.png)