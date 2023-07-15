#ifdef __cplusplus
extern "C" {
#endif

#ifdef _WIN32
#  ifdef MODULE_API_EXPORTS
#    define MODULE_API __declspec(dllexport)
#  else
#    define MODULE_API __declspec(dllimport)
#  endif
#else
#  define MODULE_API
#endif

MODULE_API struct ThumbnailResult
{
    char *Error;
    int Width;
    int Height;
};

MODULE_API struct CenterResult
{
    char *Error;
    int X;
    int Y;
};

MODULE_API struct ImageDimensions
{
    char *Error;
    int Width;
    int Height;
};

MODULE_API bool VipsInit();
MODULE_API CenterResult Center(const char *location);
MODULE_API ImageDimensions GetDimensions(const char *srcLocation);
MODULE_API ThumbnailResult ThumbnailJpeg(const char *srcLocation, const char *destLocation, int width, int height, int quality);
MODULE_API ThumbnailResult ThumbnailWebP(const char *srcLocation, const char *destLocation, int width, int height, int quality);
MODULE_API ThumbnailResult ThumbnailAvif(const char *srcLocation, const char *destLocation, int width, int height, int quality);
MODULE_API ThumbnailResult ThumbnailPpm(const char *srcLocation, const char *destLocation, int width, int height);

#ifdef __cplusplus
}
#endif