namespace PluginMzTab.Lib.Utils.Errors{
    public class FormatErrorType : MZTabErrorType{
        public static MZTabErrorType LinePrefix = createError(Category.Format, "LinePrefix");
        public static MZTabErrorType CountMatch = createError(Category.Format, "CountMatch");

        public static MZTabErrorType IndexedElement = createError(Category.Format, "IndexedElement");
        public static MZTabErrorType AbundanceColumn = createError(Category.Format, "AbundanceColumn");
        public static MZTabErrorType MsRunOptionalColumn = createError(Category.Format, "MsRunOptionalColumn");
        public static MZTabErrorType OptionalCVParamColumn = createError(Category.Format, "OptionalCVParamColumn");
        public static MZTabErrorType StableColumn = createError(Category.Format, "StableColumn");

        public static MZTabErrorType MTDLine = createError(Category.Format, "MTDLine");
        public static MZTabErrorType MTDDefineLabel = createError(Category.Format, "MTDDefineLabel");
        public static MZTabErrorType MZTabMode = createError(Category.Format, "MZTabMode");
        public static MZTabErrorType MZTabType = createError(Category.Format, "MZTabType");
        public static MZTabErrorType Param = createError(Category.Format, "Param");
        public static MZTabErrorType ParamList = createError(Category.Format, "ParamList");
        public static MZTabErrorType Publication = createError(Category.Format, "Publication");
        public static MZTabErrorType URI = createError(Category.Format, "URI");
        public static MZTabErrorType URL = createError(Category.Format, "URL");
        public static MZTabErrorType Email = createError(Category.Format, "Email");

        public static MZTabErrorType Integer = createError(Category.Format, "Integer");
        public static MZTabErrorType Double = createError(Category.Format, "Double");
        public static MZTabErrorType Reliability = createError(Category.Format, "Reliability");
        public static MZTabErrorType StringList = createError(Category.Format, "StringList");
        public static MZTabErrorType DoubleList = createError(Category.Format, "DoubleList");
        public static MZTabErrorType ModificationList = createError(Category.Format, "ModificationList");
        public static MZTabErrorType GOTermList = createError(Category.Format, "GOTermList");
        public static MZTabErrorType MZBoolean = createError(Category.Format, "MZBoolean");
        public static MZTabErrorType SpectraRef = createError(Category.Format, "SpectraRef");
        public static MZTabErrorType CHEMMODSAccession = createError(Category.Format, "CHEMMODSAccession");
        public static MZTabErrorType SearchEngineScore = createWarn(Category.Format, "SearchEngineScore");
        public static MZTabErrorType Sequence = createWarn(Category.Format, "SearchEngineScore");

        public static MZTabErrorType ColUnit = createError(Category.Format, "ColUnit");
    }
}