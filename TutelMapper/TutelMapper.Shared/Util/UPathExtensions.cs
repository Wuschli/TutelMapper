using Zio;

namespace TutelMapper.Util;

public static class UPathExtensions
{
    public static UPath GetFullPathWithoutExtension(this UPath path)
    {
        var extension = path.GetExtensionWithDot();
        if (string.IsNullOrEmpty(extension))
            return path;
        return path.FullName.Substring(0, path.FullName.Length - extension!.Length);
    }
}