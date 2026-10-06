## v1.4.0 (minor)

Changes since v1.3.0:

- Keep an exact match above the same text with a suffix [patch] ([@Claude](https://github.com/Claude))
- Give matches after -, ., /, \ and any whitespace the separator bonus [minor] ([@Claude](https://github.com/Claude))
- Ignore case for letters outside the Basic Multilingual Plane [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Charge a losing rematch as a skipped letter [patch] ([@Claude](https://github.com/Claude))
- Move CI onto the shared ci-shared.yml pipeline ([@Claude](https://github.com/Claude))
- Normalize the well-formed text around a lone surrogate [patch] ([@Claude](https://github.com/Claude))
- Match Greek final sigma and the micro sign ignoring case [patch] ([@Claude](https://github.com/Claude))
- fix: make maxPrefixPenalty actually cap the prefix penalty [patch] ([@Claude](https://github.com/Claude))
- fix: count the prefix penalty in codepoints, not UTF-16 code units [patch] ([@Claude](https://github.com/Claude))
- docs: document only the API the library actually ships [patch] ([@Claude](https://github.com/Claude))
- Gate Dependabot auto-merge on CI actually being green ([@Claude](https://github.com/Claude))
- fix: match by Unicode codepoint so a lone surrogate cannot match half a pair [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- fix: keep the Contains overloads adjacent [patch] ([@Claude](https://github.com/Claude))
- fix: match canonically equivalent NFC and NFD text [patch] ([@Claude](https://github.com/Claude))
- ci: adopt the consolidated .NET workflow [patch] ([@Claude](https://github.com/Claude))

