

select convert(nvarchar(30),getdate()-1,111);


select  * from resulthistories t1 
inner join resulthistories t2  on t1.resultId = t2.resultId and  t1.Price < t2.Price  where t1.resultdatetime > convert(nvarchar(30),getdate(),111) and 

t2.resultdatetime < convert(nvarchar(30),getdate(),111) ; 