cd Assets/Resources
find . ! -name '*.meta' ! -name '.DS_Store' | sort --ignore-case > dirlist.txt
cd ..
