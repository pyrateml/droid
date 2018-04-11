using System;
using System.Text;
using UnityEditor;
using UnityEngine;

/**
 * INSTRUCTIONS
 *
 *  - Only modify properties in the USER SETTINGS region.
 *	- All content is loaded from external files (pc_AboutEntry_YourProduct.  Use the templates!
 */

/**
 * Used to pop up the window on import.
 */
public class pg_AboutWindowSetup : AssetPostprocessor {
  #region Initialization

  static void OnPostprocessAllAssets(
      string[] importedAssets,
      string[] deletedAssets,
      string[] movedAssets,
      string[] movedFromAssetPaths) {
    var entries = Array.FindAll(
        importedAssets,
        name => name.Contains("pc_AboutEntry") && !name.EndsWith(".meta"));

    foreach (var str in entries) {
      if (pg_AboutWindow.Init(str, false))
        break;
    }
  }

  // [MenuItem("Edit/Preferences/Clear About Version: " + AboutWindow.PRODUCT_IDENTIFIER)]
  // public static void MenuClearVersionPref()
  // {
  // 	EditorPrefs.DeleteKey(AboutWindow.PRODUCT_IDENTIFIER);
  // }

  #endregion
}

public class pg_AboutWindow : EditorWindow {
/**
 * Modify these constants to customize about screen.
 */

  #region User Settings

  /* Path to the root folder */
  const string ABOUT_ROOT = "Assets/ProCore/ProGrids/About";

  /**
   * Changelog.txt file should follow this format:
   *
   *	| -- Product Name 2.1.0 -
   *	|
   *	| # Features
   *	| 	- All kinds of awesome stuff
   *	| 	- New flux capacitor design achieves time travel at lower velocities.
   *	| 	- Dark matter reactor recalibrated.
   *	| 
   *	| # Bug Fixes
   *	| 	- No longer explodes when spacebar is pressed.
   *	| 	- Fix rolling issue in Rickmeter.
   *	| 	
   *	| # Changes
   *	| 	- Changed Blue to Red.
   *	| 	- Enter key now causes explosions.
   *
   * This path is relative to the PRODUCT_ROOT path.
   *
   * Note that your changelog may contain multiple entries.  Only the top-most
   * entry will be displayed.
   */

  /**
   * Advertisement thumb constructor is:
   * new AdvertisementThumb( PathToAdImage : string, URLToPurchase : string, ProductDescription : string )
   * Provide as many or few (or none) as desired.
   *
   * Notes - The http:// part is required.  Partial URLs do not work on Mac.
   */
  [SerializeField]
  public static AdvertisementThumb[] advertisements = {
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/ProBuilder_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/probuilder/",
          "Build and Texture Geometry In-Editor"),
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/ProGrids_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/progrids/",
          "True Grids and Grid-Snapping"),
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/ProGroups_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/progroups/",
          "Hide, Freeze, Group, & Organize"),
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/Prototype_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/prototype/",
          "Design and Build With Zero Lag"),
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/QuickBrush_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/quickbrush/",
          "Quickly Add Detail Geometry"),
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/QuickDecals_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/quickdecals/",
          "Add Dirt, Splatters, Posters, etc"),
      new AdvertisementThumb(
          ABOUT_ROOT + "/Images/QuickEdit_AssetStore_Icon_96px.png",
          "http://www.protoolsforunity3d.com/quickedit/",
          "Edit Imported Meshes!")
  };

  #endregion

/* Recommend you do not modify these. */

  #region Private Fields (automatically populated)

  string AboutEntryPath = "";

  string ProductName = "";

  // private string ProductIdentifer = "";
  string ProductVersion = "";
  string ChangelogPath = "";
  readonly string BannerPath = ABOUT_ROOT + "/Images/Banner.png";

  const int AD_HEIGHT = 96;

  /**
   * Struct containing data for use in Advertisement shelf.
   */
  [Serializable]
  public struct AdvertisementThumb {
    public Texture2D image;
    public string url;
    public string about;
    public GUIContent guiContent;

    public AdvertisementThumb(string imagePath, string url, string about) {
      this.guiContent = new GUIContent("", about);
      this.image = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePath, typeof(Texture2D));
      this.guiContent.image = this.image;
      this.url = url;
      this.about = about;
    }
  }

  Texture2D banner;

  // populated by first entry in changelog
  string changelog = "";

  #endregion

  #region Init

  // [MenuItem("Tools/Test Search About Window", false, 0)]
  // public static void MenuInit()
  // {
  // 	// this could be slow in large projects?
  // 	string[] allFiles = System.IO.Directory.GetFiles("Assets/", "*.*", System.IO.SearchOption.AllDirectories);
  // 	string[] entries = System.Array.FindAll(allFiles, name => name.Contains("pc_AboutEntry"));

  // 	if(entries.Length > 0)
  // 		AboutWindow.Init(entries[0], true);
  // }

  /**
   * Return true if Init took place, false if not.
   */
  public static bool Init(string aboutEntryPath, bool fromMenu) {
    string identifier, version;

    if (!GetField(aboutEntryPath, "version: ", out version)
        || !GetField(aboutEntryPath, "identifier: ", out identifier))
      return false;

    if (fromMenu || EditorPrefs.GetString(identifier) != version) {
      string tname;

      pg_AboutWindow win;

      if (!GetField(aboutEntryPath, "name: ", out tname) || !tname.Contains("ProGrids"))
        return false;

      win = (pg_AboutWindow)GetWindow(typeof(pg_AboutWindow), true, tname, true);
      win.SetAboutEntryPath(aboutEntryPath);
      win.ShowUtility();

      EditorPrefs.SetString(identifier, version);

      return true;
    }

    return false;
  }

  public void OnEnable() {
    this.banner = (Texture2D)AssetDatabase.LoadAssetAtPath(this.BannerPath, typeof(Texture2D));

    // With Unity 4 (on PC) if you have different values for minSize and maxSize,
    // they do not apply restrictions to window size.
    this.minSize = new Vector2(this.banner.width + 12, this.banner.height * 7);
    this.maxSize = new Vector2(this.banner.width + 12, this.banner.height * 7);
  }

  public void SetAboutEntryPath(string path) {
    this.AboutEntryPath = path;
    this.PopulateDataFields(this.AboutEntryPath);
  }

  #endregion

  #region GUI

  readonly Color LinkColor = new Color(0f, .682f, .937f, 1f);

  GUIStyle boldTextStyle, headerTextStyle, linkTextStyle;

  GUIStyle advertisementStyle;

  Vector2 scroll = Vector2.zero, adScroll = Vector2.zero;

  // int mm = 32;
  void OnGUI() {
    this.headerTextStyle = this.headerTextStyle ?? new GUIStyle(EditorStyles.boldLabel); //GUI.skin.label);
    this.headerTextStyle.fontSize = 16;

    this.linkTextStyle = this.linkTextStyle ?? new GUIStyle(GUI.skin.label); //GUI.skin.label);
    this.linkTextStyle.normal.textColor = this.LinkColor;
    this.linkTextStyle.alignment = TextAnchor.MiddleLeft;

    this.boldTextStyle = this.boldTextStyle ?? new GUIStyle(GUI.skin.label); //GUI.skin.label);
    this.boldTextStyle.fontStyle = FontStyle.Bold;
    this.boldTextStyle.alignment = TextAnchor.MiddleLeft;

    // #if UNITY_4
    // richTextLabel.richText = true;
    // #endif

    this.advertisementStyle = this.advertisementStyle ?? new GUIStyle(GUI.skin.button);
    this.advertisementStyle.normal.background = null;

    if (this.banner != null)
      GUILayout.Label(this.banner);

    // mm = EditorGUI.IntField(new Rect(Screen.width - 200, 100, 200, 18), "W: ", mm);

    {
      GUILayout.Label(
          "Thank you for purchasing "
          + this.ProductName
          + ". Your support allows us to keep developing this and future tools for everyone.",
          EditorStyles.wordWrappedLabel);
      GUILayout.Space(2);
      GUILayout.Label("Read these quick \"ProTips\" before starting:", this.headerTextStyle);

      GUILayout.BeginHorizontal();
      GUILayout.Label("1) ", GUILayout.MinWidth(16), GUILayout.MaxWidth(16));
      GUILayout.Label("Register", this.boldTextStyle, GUILayout.MinWidth(58), GUILayout.MaxWidth(58));
      GUILayout.Label(
          "for instant email updates, send your invoice # to",
          GUILayout.MinWidth(284),
          GUILayout.MaxWidth(284));
      if (GUILayout.Button(
          "contact@procore3d.com",
          this.linkTextStyle,
          GUILayout.MinWidth(142),
          GUILayout.MaxWidth(142)))
        Application.OpenURL("mailto:contact@procore3d.com?subject=Sign me up for the Beta!");
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("2) ", GUILayout.MinWidth(16), GUILayout.MaxWidth(16));
      GUILayout.Label("Report bugs", this.boldTextStyle, GUILayout.MinWidth(82), GUILayout.MaxWidth(82));
      GUILayout.Label("to the ProCore Forum at", GUILayout.MinWidth(144), GUILayout.MaxWidth(144));
      if (GUILayout.Button(
          "www.procore3d.com/forum",
          this.linkTextStyle,
          GUILayout.MinWidth(162),
          GUILayout.MaxWidth(162)))
        Application.OpenURL("http://www.procore3d.com/forum");
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("3) ", GUILayout.MinWidth(16), GUILayout.MaxWidth(16));
      GUILayout.Label("Customize!", this.boldTextStyle, GUILayout.MinWidth(74), GUILayout.MaxWidth(74));
      GUILayout.Label(
          "Click on \"Edit > Preferences\" then \"" + this.ProductName + "\"",
          GUILayout.MinWidth(276),
          GUILayout.MaxWidth(276));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("4) ", GUILayout.MinWidth(16), GUILayout.MaxWidth(16));
      GUILayout.Label("Documentation", this.boldTextStyle, GUILayout.MinWidth(102), GUILayout.MaxWidth(102));
      GUILayout.Label("Tutorials, & more info:", GUILayout.MinWidth(132), GUILayout.MaxWidth(132));
      if (GUILayout.Button(
          "www.procore3d.com/" + this.ProductName.ToLower(),
          this.linkTextStyle,
          GUILayout.MinWidth(190),
          GUILayout.MaxWidth(190)))
        Application.OpenURL("http://www.procore3d.com/" + this.ProductName.ToLower());
      GUILayout.EndHorizontal();

      GUILayout.Space(4);

      GUILayout.BeginHorizontal(GUILayout.MaxWidth(50));

      GUILayout.Label("Links:", this.boldTextStyle);

      this.linkTextStyle.fontStyle = FontStyle.Italic;
      this.linkTextStyle.alignment = TextAnchor.MiddleCenter;

      if (GUILayout.Button("procore3d.com", this.linkTextStyle))
        Application.OpenURL("http://www.procore3d.com");

      if (GUILayout.Button("facebook", this.linkTextStyle))
        Application.OpenURL("http://www.facebook.com/probuilder3d");

      if (GUILayout.Button("twitter", this.linkTextStyle))
        Application.OpenURL("http://www.twitter.com/probuilder3d");

      this.linkTextStyle.fontStyle = FontStyle.Normal;
      GUILayout.EndHorizontal();

      GUILayout.Space(4);
    }

    this.HorizontalLine();

    // always bold the first line (cause it's the version info stuff)
    this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
    GUILayout.Label(this.ProductName + "  |  version: " + this.ProductVersion, EditorStyles.boldLabel);
    GUILayout.Label("\n" + this.changelog);
    EditorGUILayout.EndScrollView();

    this.HorizontalLine();

    GUILayout.Label("More ProCore Products", EditorStyles.boldLabel);

    var pad = advertisements.Length * AD_HEIGHT > Screen.width ? 22 : 6;
    this.adScroll = EditorGUILayout.BeginScrollView(
        this.adScroll,
        false,
        false,
        GUILayout.MinHeight(AD_HEIGHT + pad),
        GUILayout.MaxHeight(AD_HEIGHT + pad));
    GUILayout.BeginHorizontal();

    foreach (var ad in advertisements) {
      if (ad.url.ToLower().Contains(this.ProductName.ToLower()))
        continue;

      if (GUILayout.Button(
          ad.guiContent,
          this.advertisementStyle,
          GUILayout.MinWidth(AD_HEIGHT),
          GUILayout.MaxWidth(AD_HEIGHT),
          GUILayout.MinHeight(AD_HEIGHT),
          GUILayout.MaxHeight(AD_HEIGHT)))
        Application.OpenURL(ad.url);
    }

    GUILayout.EndHorizontal();
    EditorGUILayout.EndScrollView();
    /* shill other products */
  }

  /**
   * Draw a horizontal line across the screen and update the guilayout.
   */
  void HorizontalLine() {
    var r = GUILayoutUtility.GetLastRect();
    var og = GUI.backgroundColor;
    GUI.backgroundColor = Color.black;
    GUI.Box(new Rect(0f, r.y + r.height + 2, Screen.width, 2f), "");
    GUI.backgroundColor = og;

    GUILayout.Space(6);
  }

  #endregion

  #region Data Parsing

  /* rich text ain't wuurkin' in unity 3.5 */
  const string RemoveBraketsRegex = "(\\<.*?\\>)";

  /**
   * Open VersionInfo and Changelog and pull out text to populate vars for OnGUI to display.
   */
  void PopulateDataFields(string entryPath) {
    /* Get data from VersionInfo.txt */
    var versionInfo = (TextAsset)AssetDatabase.LoadAssetAtPath(entryPath, typeof(TextAsset));

    this.ProductName = "";
    // ProductIdentifer = "";
    this.ProductVersion = "";
    this.ChangelogPath = "";

    if (versionInfo != null) {
      var txt = versionInfo.text.Split('\n');
      foreach (var cheese in txt) {
        if (cheese.StartsWith("name:"))
          this.ProductName = cheese.Replace("name: ", "").Trim();
        else if (cheese.StartsWith("version:"))
          this.ProductVersion = cheese.Replace("version: ", "").Trim();
        else if (cheese.StartsWith("changelog:"))
          this.ChangelogPath = cheese.Replace("changelog: ", "").Trim();
      }
    }

    // notes = notes.Trim();

    /* Get first entry in changelog.txt */
    var changelogText = (TextAsset)AssetDatabase.LoadAssetAtPath(this.ChangelogPath, typeof(TextAsset));

    if (changelogText) {
      var split = changelogText.text.Split(new[] {"--"}, StringSplitOptions.RemoveEmptyEntries);
      var sb = new StringBuilder();
      var newLineSplit = split[0].Trim().Split('\n');
      for (var i = 2; i < newLineSplit.Length; i++)
        sb.AppendLine(newLineSplit[i]);

      this.changelog = sb.ToString();
    }
  }

  static bool GetField(string path, string field, out string value) {
    var entry = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
    value = "";

    if (!entry) return false;

    foreach (var str in entry.text.Split('\n')) {
      if (str.Contains(field)) {
        value = str.Replace(field, "").Trim();
        return true;
      }
    }

    return false;
  }

  #endregion
}
