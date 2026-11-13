using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class OrderDocumentTypeDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("source_video", "Source video"),
        new("source_accompaniment_audio", "Source accompaniment audio"),
        new("source_srt", "Source subtitles"),
        new("source_document", "Source document"),
        new("created_source_vocals_only_audio", "Created source vocals"),
        new("created_waveform", "Created waveform"),
        new("created_accompaniment_audio", "Created accompaniment audio"),
        new("created_ai_dub_audio", "Created AI dub audio"),
        new("created_ai_dub_vocals_only_audio", "Created AI dub vocals"),
        new("created_embedded_video", "Created embedded video"),
        new("created_subtitle_embedded_video", "Created subtitle embedded video"),
        new("created_compressed_video", "Created compressed video"),
        new("created_thumbnail", "Created thumbnail"),
        new("vendor_audio_with_background", "Vendor audio with background"),
        new("vendor_audio_without_background", "Vendor audio without background"),
        new("vendor_video", "Vendor video"),
        new("translator_document", "Translator document"),
        new("guideline", "Guideline"),
        new("character_list", "Character list"),
        new("guideline_glossary", "Glossary")
    ];
}
