import os

def parse_vdf(file_path):
    translations = {}
    with open(file_path, 'r', encoding='utf-8') as file:
        lines = file.readlines()
        in_tokens = False
        for line in lines:
            line = line.strip()
            if line.startswith('"Tokens"'):
                in_tokens = True
                continue
            if in_tokens and line.startswith('}'):
                break
            if in_tokens:
                parts = line.split('\t')
                if len(parts) == 2:
                    key = parts[0].strip().strip('"')
                    value = parts[1].strip().strip('"')
                    translations[key] = value
    return translations

def main():
    print('Starting...')
    english_file = '1916890_loc_english.vdf'
    korean_file = '1916890_loc_korean.vdf'
    
    english_translations = parse_vdf(english_file)
    korean_translations = parse_vdf(korean_file)
    
    translation_dict = {}
    for key in english_translations:
        if key in korean_translations:
            translation_dict[english_translations[key]] = korean_translations[key]
    
    print(translation_dict)
    
    # Read Xbox English XML file
    xbox_english_path = r'C:\Users\rando\OneDrive\Documents\Game Dev\Slider\production\misc\xbox_loc_english.xml'
    xbox_korean_path = r'C:\Users\rando\OneDrive\Documents\Game Dev\Slider\production\misc\xbox_loc_korean.xml'
    
    with open(xbox_english_path, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Replace English text with Korean translations
    for english_text, korean_text in translation_dict.items():
        content = content.replace(english_text, korean_text)

    content = content.replace('en-US', 'ko-KR')
    
    # Write Korean XML file
    with open(xbox_korean_path, 'w', encoding='utf-8') as file:
        file.write(content)
    
    with open(xbox_english_path, 'r', encoding='utf-8') as file:
        lines = file.readlines()
    
    with open(os.path.join(os.path.dirname(xbox_english_path), 'xbox_loc.xml'), 'w', encoding='utf-8') as output_file:
        for i, line in enumerate(lines):
            output_file.write(line)
            if 'Value locale="en-US"' in line:
                # Find corresponding Korean line
                korean_line = line.replace('en-US', 'ko-KR')
                for english_text, korean_text in translation_dict.items():
                    korean_line = korean_line.replace(english_text, korean_text)
                output_file.write(korean_line)

if __name__ == "__main__":
    main()