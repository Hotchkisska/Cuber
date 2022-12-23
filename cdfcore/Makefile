all:
	csc cdfcore.cs
dll:
	csc /target:library /out:CDFcore.dll cdfcore.cs
shell:
	csc /r:CDFcore.dll cdf.cs