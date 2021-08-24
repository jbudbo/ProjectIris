namespace Worker.Models
{
    internal sealed record EmojiData(
        string name,
        string unified,
        string non_qualified,
        string docomo,
        string au,
        string softbank,
        string google,
        string image,
        int sheet_x,
        int sheet_y,
        string short_name,
        string[] short_names,
        string text,
        string[] texts,
        string category,
        string subcategory,
        int sort_order,
        string added_in,
        bool has_img_apple,
        bool has_img_google,
        bool has_img_twitter,
        bool has_img_facebook);
}
