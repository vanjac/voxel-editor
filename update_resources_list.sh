cd Assets/Resources/GameAssets
find . ! -name '*.meta' ! -name '.DS_Store' ! -name "dirlist.txt" | sort --ignore-case > dirlist.txt
cd ..
