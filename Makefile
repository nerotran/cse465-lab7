

all: lex.exe

check: check-example1

check-example1: example1.output
	cmp example1.output example1.expected

example1.output: lex.exe example1.input
	mono lex.exe < example1.input >example1.output

lex.exe: lex.cs
	csc lex.cs


submit: check
	git commit -am "Submitting"
	git push origin master

update-http:
	git pull https://gitlab.csi.miamioh.edu/CSE465/instructor/lab03.git

update-ssh:
	git pull git@gitlab.csi.miamioh.edu:CSE465/instructor/lab03.git

update: update-http
