

create index i1 on xresulthistories (price,resultdatetime);


select count(*) from results;
select count(*) from xresulthistories;


/*select  t3.id ,t3.PublicRemaRrks, t3.PostalCode , t3.RelativeDetailsURL from  results  t3 where t3.RelativeDetailsURL like '%calgary%';  */


select  t1.id, t1.ResultId,t1.resultdatetime, t1.Price,t2.resultdatetime, t2.Price, t3.PostalCode , t3.RelativeDetailsURL from resulthistories t1 
inner join xresulthistories t2  on t1.resultId = t2.resultId and  t1.Price < t2.Price  join results t3 on t1.resultid = t3.id where t1.resultdatetime > convert(nvarchar(30),getdate(),111) and 

t2.resultdatetime > convert(nvarchar(30),getdate()-1,111) and t2.resultdatetime < convert(nvarchar(30),getdate(),111) and  t3.RelativeDetailsURL like '%calgary%';



/*
select   t1.id, t1.ResultId,t1.resultdatetime, t1.Price,t2.resultdatetime, t2.Price,  t3.PostalCode , t3.RelativeDetailsURL  from resulthistories t1 
inner join resulthistories t2  on t1.resultId = t2.resultId and  t1.Price < t2.Price  join results t3 on t1.resultid = t3.id where t1.resultdatetime > convert(nvarchar(30),getdate(),111) and 

t2.resultdatetime > convert(nvarchar(30),getdate()-1,111) and t2.resultdatetime < convert(nvarchar(30),getdate(),111) and   t3.RelativeDetailsURL like '%albert%';
*/

/*

select  t1.id, t1.ResultId,t1.resultdatetime, t1.Price,t2.resultdatetime, t2.Price, t3.PostalCode , t3.RelativeDetailsURL from resulthistories t1 
inner join resulthistories t2  on t1.resultId = t2.resultId and  t1.Price < t2.Price  join results t3 on t1.resultid = t3.id where t1.resultdatetime > convert(nvarchar(30),getdate(),111) and 

t2.resultdatetime > convert(nvarchar(30),getdate()-1,111) and t2.resultdatetime < convert(nvarchar(30),getdate(),111) ;

*/