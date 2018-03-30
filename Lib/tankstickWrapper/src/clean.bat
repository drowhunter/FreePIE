@echo Cleaning %1
del /Q /F *.suo
del /Q /F *.bak

for /d /r . %%d in (bin,obj,Debug,Release) do (
	@if exist "%%d" (
		@echo cleaning %%d
		rd /s/q "%%d"
	)
)
