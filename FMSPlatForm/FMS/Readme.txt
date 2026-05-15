2011-08-23 
(1) FMS platform upgrade to 1.3
(2) Add crawler service for SMT Stage 3 Project
(3) The timer of Interval change to second (ms to s)
(4) Solve potential bug in fmDLConfig and fmFMSMain

2011-02-25
(1) FMS platform upgrade to 1.2.0.12 by Keven Peng
(2) Add logic monitor mode
(3) Add FTASSY
   FT instance class & library mapping
   instance class : Compal.FMS.ASSYFTImpl.ASSYFTClient
   library : ASSYFTImpl.dll

2010-12-05 : 
 SMT FT instance class & library mapping
   instance class : Compal.FMS.SMTFTImpl.SMTClient
   library : SMTFTImpl.dll
 
 A31 AFT instance class & library mapping
   instance class : Compal.FMS.A31AFTImpl.A31AFTClient
   library : A31AFT.dll

2010-11-25
(1) upgrade to 1.2.0.11
    - separate one module to multiple module
    - FMS (UI module), FMSCommon (Kernel module), 
	  A31AFT (customer module) SMT FT (customer module)
	- upgrade to .NET 3.5
