## v1.4.2 (patch)

Changes since v1.4.1:

- Match the Turkish dotted and dotless I ignoring case [patch] (#136) (matt-edmondson)
- Classify letter case by Unicode category as well as case mapping [patch] (#137) (matt-edmondson)
- Score a combining mark as part of the letter before it [patch] (#139) (matt-edmondson)
- Give a capital after a digit the camelCase bonus [patch] (#132) (matt-edmondson)
- Treat a losing rematch as unmatched for the next letter [patch] (#131) (matt-edmondson)
- Regenerate CompatibilitySuppressions.xml after the Polyfill 11.4.5 bump [patch] (#129) (matt-edmondson)
- Give supplementary-plane capitals the camelCase bonus [patch] (Claude)
- Share one TryNormalize between whole-text and per-run normalization (Claude)
- Keep U+FFFE from throwing out of Contains [patch] (Claude)
- Let a boundary copy displace a letter that only follows its own copy [patch] (Claude)

